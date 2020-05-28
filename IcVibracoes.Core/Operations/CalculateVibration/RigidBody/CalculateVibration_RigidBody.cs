using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder;
using IcVibracoes.Core.Operations.CalculateVibration;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration for a rigid body analysis.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public abstract class CalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData, TInput> : CalculateVibration<TRequest, TRequestData, TResponse, TResponseData, TInput>, ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData, TInput>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
        where TInput : RigidBodyInput, new()
    {
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IRungeKuttaForthOrderMethod<TInput> _rungeKutta;
        private readonly ITime _time;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibration_RigidBody(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod<TInput> rungeKutta,
            ITime time)
        {
            this._auxiliarOperation = auxiliarOperation;
            this._rungeKutta = rungeKutta;
            this._time = time;
        }

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task<double[]> BuildInitialConditions(TRequestData data);

        protected override async Task<TResponse> ProcessOperation(TRequest request)
        {
            var response = new TResponse();

            double[] initial_y = await this.BuildInitialConditions(request.Data).ConfigureAwait(false);

            TInput input = await this.CreateInput(request).ConfigureAwait(false);

            // Parallel.Foreach
            foreach (double dampingRatio in request.Data.DampingRatioList)
            {
                input.DampingRatio = dampingRatio;

                while (input.AngularFrequency <= input.FinalAngularFrequency)
                {
                    input.TimeStep = await this._time.CalculateTimeStep(input.Mass, input.Stiffness, input.AngularFrequency, request.Data.PeriodDivision).ConfigureAwait(false);
                    input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.Data.PeriodCount).ConfigureAwait(false);

                    string path = await this.CreateSolutionPath(request.AnalysisType, input, response).ConfigureAwait(false);

                    if (path == null)
                    {
                        return response;
                    }

                    double time = request.Data.InitialTime;
                    double[] y = initial_y;

                    this._auxiliarOperation.Write(time, y, path);

                    while (time <= input.FinalTime)
                    {
                        y = await this._rungeKutta.CalculateResult(input, input.TimeStep, time, y).ConfigureAwait(false);

                        this._auxiliarOperation.Write(time + input.TimeStep, y, path);

                        time += input.TimeStep;
                    }

                    input.AngularFrequency += input.AngularFrequencyStep;
                }
            }

            return response;
        }

        protected override Task<TResponse> ValidateOperation(TRequest request)
        {
            var response = new TResponse();

            return Task.FromResult(response);
        }
    }
}
