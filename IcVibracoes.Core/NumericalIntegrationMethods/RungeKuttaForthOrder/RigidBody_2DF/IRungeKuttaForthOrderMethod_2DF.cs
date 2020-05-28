using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration in caso of two degrees freedom.
    /// </summary>
    public interface IRungeKuttaForthOrderMethod_2DF : IRungeKuttaForthOrderMethod<TwoDegreesOfFreedomInput>
    { }
}