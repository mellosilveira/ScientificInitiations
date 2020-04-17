using IcVibracoes.DataContracts.RigidBody;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It is responsible to calculate vibration in rigid body case.
    /// </summary>
    /// <typeparam name="TRequestData"></typeparam>
    public interface ICalculateVibration_RigidBody<TRequestData> : IOperationBase<RigidBodyRequest<TRequestData>, RigidBodyResponse>
        where TRequestData : RigidBodyRequestData, new()
    { }
}
