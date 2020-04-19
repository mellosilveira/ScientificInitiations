using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in case of one degree freedom.
    /// </summary>
    public interface IRungeKuttaForthOrderMethod_1DF : IRungeKuttaForthOrderMethod<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>
    { }
}