using IcVibracoes.Common.Classes;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.MechanicalProperties
{
    /// <summary>
    /// It is responsible to validate the mechanical properties used in rigid body analysis.
    /// </summary>
    public class MechanicalPropertiesValidator : IMechanicalPropertiesValidator
    {
        /// <summary>
        /// This method validates the mechanical properties used in rigid body analysis.
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="mechanicalProperty"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public Task Execute<TResponseData>(MechanicalProperty mechanicalProperty, RigidBodyResponse<TResponseData> response)
            where TResponseData : RigidBodyResponseData, new()
        {
            if(mechanicalProperty.Mass < 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Mass: {mechanicalProperty.Mass} cannot be less than zero.");
            }

            if (mechanicalProperty.Stiffness < 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Stiffness: {mechanicalProperty.Stiffness} cannot be less than zero.");
            }

            return Task.CompletedTask;
        }
    }
}
