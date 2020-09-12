using IcVibracoes.Common.Classes;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;
using IcVibracoes.Core.ExtensionMethods;

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
            response
                .AddErrorIf(() => mechanicalProperty.Mass < 0, $"Mass: {mechanicalProperty.Mass} cannot be less than zero.")
                .AddErrorIf(() => mechanicalProperty.Stiffness < 0, $"Stiffness: {mechanicalProperty.Stiffness} cannot be less than zero.");

            return Task.CompletedTask;
        }
    }
}
