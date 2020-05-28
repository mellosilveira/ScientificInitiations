using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of two degrees freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_2DF : RungeKuttaForthOrderMethod<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, IRungeKuttaForthOrderMethod_2DF
    {
        private readonly IDifferentialEquationOfMotion _calculate;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="calculate"></param>
        public RungeKuttaForthOrderMethod_2DF(
            IDifferentialEquationOfMotion calculate)
        {
            _calculate = calculate;
        }

        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            return await _calculate.ExecuteForTwoDegreedOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
