using IcVibracoes.Core.AuxiliarOperations.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of one degree freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_1DF : RungeKuttaForthOrderMethod<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, IRungeKuttaForthOrderMethod_1DF
    {
        private readonly ICalculateDifferentialEquationOfMotion _calculate;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="calculate"></param>
        public RungeKuttaForthOrderMethod_1DF(
            ICalculateDifferentialEquationOfMotion calculate)
        {
            this._calculate = calculate;
        }

        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            return await this._calculate.ExecuteForOneDegreeOfFreedom(input, time, y).ConfigureAwait(false);
        }
    }
}
