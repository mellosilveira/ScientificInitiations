using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public interface IRungeKuttaForthOrderMethod<TInput>
        where TInput : RigidBodyInput
    {
        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// For each case, with one or two degrees of freedom, there is a different differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Task<double[]> CalculateDifferencialEquationOfMotion(TInput input, double time, double[] y);

        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Task<double[]> CalculateResult(TInput input, double time, double[] y);
    }
}
