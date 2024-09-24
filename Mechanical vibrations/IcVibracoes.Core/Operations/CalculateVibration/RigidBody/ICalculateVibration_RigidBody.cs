using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Operations.CalculateVibration;
using IcVibracoes.DataContracts.RigidBody;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration using rigid body concepts.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public interface ICalculateVibration_RigidBody<TRequest, TResponse, TResponseData, TInput> : ICalculateVibration<TRequest, TResponse, TResponseData, TInput>
        where TRequest : RigidBodyRequest
        where TResponse : RigidBodyResponse<TResponseData>, new()
        where TResponseData : RigidBodyResponseData, new()
        where TInput : RigidBodyInput, new()
    { }
}