namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the response content of Rigid Body operation.
    /// </summary>
    public abstract class RigidBodyResponse<TResponseData> : OperationResponseBase<TResponseData> 
        where TResponseData : RigidBodyResponseData
    { }
}
