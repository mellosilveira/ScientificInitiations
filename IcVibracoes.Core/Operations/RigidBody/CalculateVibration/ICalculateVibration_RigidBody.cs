using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body analysis.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public interface ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData> : IOperationBase<TRequest, TResponse, TResponseData>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
    {
        /// <summary>
        /// Builds the input of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        Task<DifferentialEquationOfMotionInput> BuildDifferentialEquationOfMotionInput(TRequestData requestData);
    }
}