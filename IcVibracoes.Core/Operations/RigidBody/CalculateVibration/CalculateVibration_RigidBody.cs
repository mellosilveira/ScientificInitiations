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
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// For each case, with one or two degrees of freedom, there is a different differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y);

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
            var response = new TResponse
            {
                Data = new TResponseData()
            };

            double[] y = await this.BuildInitialConditions(request.Data);
            response.Data.Results = new Dictionary<double, double[]>
            {
                { request.Data.InitialTime, y}
            };

            double time = request.Data.InitialTime;
            double timeStep = request.Data.TimeStep;
            double finalTime = request.Data.FinalTime;

            DifferentialEquationOfMotionInput input = await this.BuildDifferentialEquationOfMotionInput(request.Data).ConfigureAwait(false);

            // Parallel.Foreach
            foreach (double dampingRatio in request.Data.DampingRatioList)
            {
                input.DampingRatio = dampingRatio;

                double w = request.Data.InitialAngularFrequency;
                double dw = request.Data.AndularFrequencyStep;
                double wf = request.Data.FinalAngularFrequency;

                while (w <= wf)
                {
                    string path = await this.CreateSolutionPath(response, request.Data, request.AnalysisType, dampingRatio, w);

                    while (time <= finalTime)
                    {
                        y = await this._rungeKutta.ExecuteMethod(input, timeStep, time, y);

                        this._auxiliarOperation.WriteInFile(time, y, path);

                        response.Data.Results.Add(time, y);

                        time += timeStep;
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
