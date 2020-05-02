using IcVibracoes.Core.AuxiliarOperations.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.TwoDegreeFreedom
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of two degrees freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_2DF : RungeKuttaForthOrderMethod<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, IRungeKuttaForthOrderMethod_2DF
    {
        private readonly ICalculateDifferentialEquationOfMotion _calculate;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="calculate"></param>
        public RungeKuttaForthOrderMethod_2DF(
            ICalculateDifferentialEquationOfMotion calculate)
        {
            this._calculate = calculate;
        }

        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            return await this._calculate.CalculateForTwonumberOfElements(input, time, y).ConfigureAwait(false);
        }
    }
}
