using IcVibracoes.DataContracts.RigidBody;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    internal interface ICalculateVibrationToOneDegreeFreedom : IOperationBase<OneDegreeFreedomRequest, RigidBodyResponse, RigidBodyResponseData>
    {
    }
}