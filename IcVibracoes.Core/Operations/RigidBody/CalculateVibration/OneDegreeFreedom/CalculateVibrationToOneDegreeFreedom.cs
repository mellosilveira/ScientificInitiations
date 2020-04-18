using IcVibracoes.DataContracts.RigidBody;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : OperationBase<OneDegreeFreedomRequest, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, ICalculateVibrationToOneDegreeFreedom
    {
        public CalculateVibrationToOneDegreeFreedom()
        {

        }

        protected override Task<OneDegreeFreedomResponse> ProcessOperation(OneDegreeFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<OneDegreeFreedomResponse> ValidateOperation(OneDegreeFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
