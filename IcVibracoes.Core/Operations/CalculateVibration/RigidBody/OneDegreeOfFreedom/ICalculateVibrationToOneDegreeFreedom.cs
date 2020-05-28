using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public interface ICalculateVibrationToOneDegreeFreedom : ICalculateVibration_RigidBody<OneDegreeOfFreedomRequest, OneDegreeOfFreedomRequestData, OneDegreeOfFreedomResponse, OneDegreeOfFreedomResponseData, OneDegreeOfFreedomInput> { }
}