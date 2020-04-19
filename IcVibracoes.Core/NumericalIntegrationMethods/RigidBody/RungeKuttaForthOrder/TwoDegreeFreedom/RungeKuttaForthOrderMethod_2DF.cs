using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.TwoDegreeFreedom
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of two degrees freedom.
    /// </summary>
    public class RungeKuttaForthOrderMethod_2DF : RungeKuttaForthOrderMethod<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, IRungeKuttaForthOrderMethod_2DF
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="operation"></param>
        public RungeKuttaForthOrderMethod_2DF(
            ICalculateVibrationToTwoDegreesFreedom operation) : base(operation)
        { }
    }
}
