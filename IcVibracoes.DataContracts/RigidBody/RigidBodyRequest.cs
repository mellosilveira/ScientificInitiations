using IcVibracoes.DataContracts.RigidBody;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the request content to Rigid Body operation.
    /// </summary>
    public abstract class RigidBodyRequest<TRequestData> : OperationRequestBase
        where TRequestData : RigidBodyRequestData
    {
        public TRequestData Data { get; set; }
    }
}
