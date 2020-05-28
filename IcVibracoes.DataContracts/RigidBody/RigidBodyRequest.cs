namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the request content to Rigid Body operation.
    /// </summary>
    public abstract class RigidBodyRequest<TRequestData> : OperationRequestBase<TRequestData>
        where TRequestData : RigidBodyRequestData
    {
    }
}
