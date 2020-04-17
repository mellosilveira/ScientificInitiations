using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It is responsible to calculate vibration in rigid body case.
    /// </summary>
    /// <typeparam name="TRequestData"></typeparam>
    public abstract class CalculateVibration_RigidBody<TRequestData> : OperationBase<RigidBodyRequest<TRequestData>, RigidBodyResponse>, ICalculateVibration_RigidBody<TRequestData>
        where TRequestData : RigidBodyRequestData, new()
    {
        public CalculateVibration_RigidBody()
        {

        }

        protected override Task<RigidBodyResponse> ProcessOperation(RigidBodyRequest<TRequestData> request)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<RigidBodyResponse> ValidateOperation(RigidBodyRequest<TRequestData> request)
        {
            throw new System.NotImplementedException();
        }
    }
}
