using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of two degrees freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_2DF : RungeKuttaForthOrderMethod, IRungeKuttaForthOrderMethod_2DF
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

        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            return await this._differentialEquationOfMotion.CalculateForTwoDegreedOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
