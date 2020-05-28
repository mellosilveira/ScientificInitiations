using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public interface IRungeKuttaForthOrderMethod
    {
        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="timeStep"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Task<double[]> CalculateResult(DifferentialEquationOfMotionInput input, double timeStep, double time, double[] y);
    }
}
