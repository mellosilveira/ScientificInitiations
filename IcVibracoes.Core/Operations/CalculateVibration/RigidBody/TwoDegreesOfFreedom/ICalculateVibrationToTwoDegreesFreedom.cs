using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public interface ICalculateVibrationToTwoDegreesFreedom : ICalculateVibration_RigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomRequestData, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput> { }
}