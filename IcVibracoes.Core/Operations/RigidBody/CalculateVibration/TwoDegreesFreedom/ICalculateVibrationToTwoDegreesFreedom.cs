using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public interface ICalculateVibrationToTwoDegreesFreedom : ICalculateVibration_RigidBody<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>
    { }
}