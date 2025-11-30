using IcVibracoes.Common.Classes;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.MechanicalProperties
{
    /// <summary>
    /// It is responsible to validate the mechanical properties used in rigid body analysis.
    /// </summary>
    public interface IMechanicalPropertiesValidator
    {
        /// <summary>
        /// This method validates the mechanical properties used in rigid body analysis.
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="mechanicalProperty"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task Execute<TResponseData>(MechanicalProperty mechanicalProperty, RigidBodyResponse<TResponseData> response)
            where TResponseData : RigidBodyResponseData, new();
    }
}