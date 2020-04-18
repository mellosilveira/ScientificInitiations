using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public class CalculateVibrationToTwoDegreesFreedom : OperationBase<TwoDegreesFreedomRequest, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, ICalculateVibrationToTwoDegreesFreedom
    {
        public CalculateVibrationToTwoDegreesFreedom()
        {

        }

        protected override Task<TwoDegreesFreedomResponse> ProcessOperation(TwoDegreesFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<TwoDegreesFreedomResponse> ValidateOperation(TwoDegreesFreedomRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
