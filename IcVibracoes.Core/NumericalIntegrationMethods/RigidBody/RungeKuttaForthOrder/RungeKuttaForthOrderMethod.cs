using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public abstract class RungeKuttaForthOrderMethod<TRequest, TRequestData, TResponse, TResponseData> : IRungeKuttaForthOrderMethod<TRequest, TRequestData, TResponse, TResponseData>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
    {
        private readonly ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData> _operation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="operation"></param>
        public RungeKuttaForthOrderMethod(
            ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData> operation)
        {
            this._operation = operation;
        }

        public async Task<TResponse> CalculateResponse(TRequest request, int numberOfEquations)
        {
            var response = new TResponse();

            double time = request.Data.InitialTime;
            double dt = request.Data.TimeStep;
            double finalTime = request.Data.FinalTime;

            // Parallel.Foreach
            foreach (var dampingRatio in request.Data.DampingRatioList)
            {
                while (time <= finalTime)
                {
                    double[] y = new double[numberOfEquations];



                    time += dt;
                }
            }

            return response;
        }

        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="timeStep"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> ExecuteMethod(DifferentialEquationOfMotionInput input, double timeStep, double time, double[] y)
        {
            int arrayLength = y.Length;

            double[] result = new double[arrayLength];
            double[] t1 = new double[arrayLength];
            double[] t2 = new double[arrayLength];
            double[] t3 = new double[arrayLength];

            double[] y1 = await this._operation.CalculateDifferencialEquationOfMotion(input, time, y).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t1[i] = y[i] + 0.5 * timeStep * y1[i];
            }

            double[] y2 = await this._operation.CalculateDifferencialEquationOfMotion(input, time + timeStep / 2, t1).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t2[i] = y[i] + 0.5 * timeStep * y2[i];
            }

            double[] y3 = await this._operation.CalculateDifferencialEquationOfMotion(input, time + timeStep / 2, t2).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t3[i] = y[i] + timeStep * y3[i];
            }

            double[] y4 = await this._operation.CalculateDifferencialEquationOfMotion(input, time + timeStep, t3).ConfigureAwait(false);
            
            for (int i = 0; i < arrayLength; i++)
            {
                result[i] = (y1[i] + 2 * y2[i] + 2 * y3[i] + y4[i]) * (timeStep / 6);
            }

            return result;
        }
    }
}
