using IcVibracoes.DataContracts.RigidBody;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    public class CalculateVibrationToOneDegreeFreedom : OperationBase<OneDegreeFreedomRequest, RigidBodyResponse, RigidBodyResponseData>, ICalculateVibrationToOneDegreeFreedom
    {
        public CalculateVibrationToOneDegreeFreedom()
        {

        }

        protected override Task<RigidBodyResponse> ProcessOperation(OneDegreeFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<RigidBodyResponse> ValidateOperation(OneDegreeFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
