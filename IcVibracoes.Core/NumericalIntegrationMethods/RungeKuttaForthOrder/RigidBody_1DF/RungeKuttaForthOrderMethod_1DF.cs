using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_1DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of one degree freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_1DF : RungeKuttaForthOrderMethod<OneDegreeOfFreedomInput>, IRungeKuttaForthOrderMethod_1DF
    {
        private readonly IDifferentialEquationOfMotion _differentialEquationOfMotion;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="differentialEquationOfMotion"></param>
        public RungeKuttaForthOrderMethod_1DF(
            IDifferentialEquationOfMotion differentialEquationOfMotion)
        {
            this._differentialEquationOfMotion = differentialEquationOfMotion;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time to one degree of freedom, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateDifferencialEquationOfMotion(OneDegreeOfFreedomInput input, double time, double[] y)
        {
            return await this._differentialEquationOfMotion.CalculateForOneDegreeOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
