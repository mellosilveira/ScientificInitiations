using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public interface ICalculateVibrationToOneDegreeFreedom : ICalculateVibration_RigidBody<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>
    { }
}