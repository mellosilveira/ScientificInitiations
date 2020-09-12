using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom.
    /// </summary>
    public interface ICalculateVibrationToTwoDegreesOfFreedom : ICalculateVibrationRigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput> { }
}