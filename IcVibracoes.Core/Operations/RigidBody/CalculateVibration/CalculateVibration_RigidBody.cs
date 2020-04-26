using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder;
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
        private readonly IRungeKuttaForthOrderMethod<TRequest, TRequestData, TResponse, TResponseData> _rungeKutta;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibration_RigidBody(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod<TRequest, TRequestData, TResponse, TResponseData> rungeKutta)
        {
            this._auxiliarOperation = auxiliarOperation;
            this._rungeKutta = rungeKutta;
        }

        /// <summary>
        /// Builds the input of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public abstract Task<DifferentialEquationOfMotionInput> BuildDifferentialEquationOfMotionInput(TRequestData requestData);

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
            double timeStep = request.Data.TimeStep;
            double finalTime = request.Data.FinalTime;

            DifferentialEquationOfMotionInput input = await this.BuildDifferentialEquationOfMotionInput(request.Data).ConfigureAwait(false);

            // Parallel.Foreach
            foreach (double dampingRatio in request.Data.DampingRatioList)
            {
                input.DampingRatio = dampingRatio;

                double dw = request.Data.AngularFrequencyStep;
                double w = request.Data.InitialAngularFrequency;
                double wf = request.Data.FinalAngularFrequency;

                while (w <= wf)
                {
                    input.AngularFrequency = w;

                    string path = await this.CreateSolutionPath(response, request.Data, request.AnalysisType, input.DampingRatio, w).ConfigureAwait(false);

                    if (path == null)
                    {
                        return response;
                    }

                    double time = request.Data.InitialTime;
                    double[] y = initial_y;

                    while (time <= finalTime)
                    {
                        if (time != request.Data.InitialTime)
                        {
                            y = await this._rungeKutta.ExecuteMethod(input, timeStep, time, y).ConfigureAwait(false);
                        }

                        this._auxiliarOperation.WriteInFile(time, y, path);

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
