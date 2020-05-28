using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_1DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of one degree freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_1DF : RungeKuttaForthOrderMethod<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, IRungeKuttaForthOrderMethod_1DF
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

        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            return await this._differentialEquationOfMotion.CalculateForOneDegreeOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
