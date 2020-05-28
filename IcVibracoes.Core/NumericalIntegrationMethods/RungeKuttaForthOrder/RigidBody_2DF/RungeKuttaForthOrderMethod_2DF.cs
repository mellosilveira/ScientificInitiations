using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of two degrees freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_2DF : RungeKuttaForthOrderMethod<TwoDegreesOfFreedomInput>, IRungeKuttaForthOrderMethod_2DF
    {
        private readonly IDifferentialEquationOfMotion _differentialEquationOfMotion;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="differentialEquationOfMotion"></param>
        public RungeKuttaForthOrderMethod_2DF(
            IDifferentialEquationOfMotion differentialEquationOfMotion)
        {
            this._differentialEquationOfMotion = differentialEquationOfMotion;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// For each case, with one or two degrees of freedom, there is a different differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateDifferencialEquationOfMotion(TwoDegreesOfFreedomInput input, double time, double[] y)
        {
            return await this._differentialEquationOfMotion.CalculateForTwoDegreedOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
