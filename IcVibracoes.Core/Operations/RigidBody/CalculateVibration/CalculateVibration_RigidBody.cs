using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder;
using IcVibracoes.DataContracts.RigidBody;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body analysis.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public abstract class CalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData> : OperationBase<TRequest, TResponse, TResponseData>, ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
    {
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IRungeKuttaForthOrderMethod _rungeKutta;
        private readonly ITime _time;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibration_RigidBody(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod rungeKutta,
            ITime time)
        {
            this._auxiliarOperation = auxiliarOperation;
            this._rungeKutta = rungeKutta;
            this._time = time;
        }

        /// <summary>
        /// Builds the input of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public abstract Task<DifferentialEquationOfMotionInput> CreateInput(TRequestData requestData);

        /// <summary>
        /// Create a path to the files with the analysis solution.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="requestData"></param>
        /// <param name="analysisType"></param>
        /// <param name="dampingRatio"></param>
        /// <param name="angularFrequency"></param>
        /// <returns></returns>
        public abstract Task<string> CreateSolutionPath(TResponse response, TRequestData requestData, string analysisType, double dampingRatio, double angularFrequency);

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

            DifferentialEquationOfMotionInput input = await this.CreateInput(request.Data).ConfigureAwait(false);

            // Parallel.Foreach
            foreach (double dampingRatio in request.Data.DampingRatioList)
            {
                input.DampingRatio = dampingRatio;

                double dw = request.Data.AngularFrequencyStep;
                double w = request.Data.InitialAngularFrequency;
                double wf = request.Data.FinalAngularFrequency;

                while (w <= wf)
                {
                    double timeStep = await this._time.CalculateTimeStep(input.Mass, input.Stiffness, input.AngularFrequency, request.Data.PeriodDivision).ConfigureAwait(false);
                    double finalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.Data.PeriodCount).ConfigureAwait(false);

                    input.AngularFrequency = w;

                    string path = await this.CreateSolutionPath(response, request.Data, request.AnalysisType, input.DampingRatio, input.AngularFrequency).ConfigureAwait(false);

                    if (path == null)
                    {
                        return response;
                    }

                    double time = request.Data.InitialTime;
                    double[] y = initial_y;
                    this._auxiliarOperation.WriteInFile(time, y, path);

                    while (time <= finalTime)
                    {
                        y = await this._rungeKutta.CalculateResult(input, timeStep, time, y).ConfigureAwait(false);

                        this._auxiliarOperation.WriteInFile(time + timeStep, y, path);

                        time += timeStep;
                    }

                    if (w == wf)
                    {
                        break;
                    }

                    w += dw;
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
