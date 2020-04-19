using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of one degree freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_1DF : RungeKuttaForthOrderMethod<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, IRungeKuttaForthOrderMethod_1DF
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="operation"></param>
        public RungeKuttaForthOrderMethod_1DF(
            ICalculateVibrationToOneDegreeFreedom operation) : base(operation)
        { }
    }
}
