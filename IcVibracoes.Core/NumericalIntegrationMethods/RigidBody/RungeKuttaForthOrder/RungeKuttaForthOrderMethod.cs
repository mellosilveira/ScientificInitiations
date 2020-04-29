using IcVibracoes.Core.DTO.InputData;
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
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="timeStep"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateResult(DifferentialEquationOfMotionInput input, double timeStep, double time, double[] y)
        {
            int arrayLength = y.Length;

            double[] result = new double[arrayLength];
            double[] t1 = new double[arrayLength];
            double[] t2 = new double[arrayLength];
            double[] t3 = new double[arrayLength];

            double[] y1 = await this.CalculateDifferencialEquationOfMotion(input, time, y).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t1[i] = y[i] + 0.5 * timeStep * y1[i];
            }

            double[] y2 = await this.CalculateDifferencialEquationOfMotion(input, time + timeStep / 2, t1).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t2[i] = y[i] + 0.5 * timeStep * y2[i];
            }

            double[] y3 = await this.CalculateDifferencialEquationOfMotion(input, time + timeStep / 2, t2).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t3[i] = y[i] + timeStep * y3[i];
            }

            double[] y4 = await this.CalculateDifferencialEquationOfMotion(input, time + timeStep, t3).ConfigureAwait(false);

            for (int i = 0; i < arrayLength; i++)
            {
                result[i] = (y1[i] + 2 * y2[i] + 2 * y3[i] + y4[i]) * (timeStep / 6);
            }

            return result;
        }
    }
}
