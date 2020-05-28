using IcVibracoes.Common.Classes;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom
{
    /// <summary>
    /// It contains the request 'data' content of Rigid Body analysis with One Degree Freedom.
    /// </summary>
    public class OneDegreeOfFreedomRequestData : RigidBodyRequestData
    {
        /// <summary>
        /// The mechanical properties of the object that will be analyzed.
        /// </summary>
        [Required]
        public MechanicalProperties MechanicalProperties { get; set; }

        /// <summary>
        /// The initial displacement.
        /// Unit: m (meter)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double InitialDisplacement { get; set; }

        /// <summary>
        /// The initial velocity.
        /// Unit: m/s (meters per second)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double InitialVelocity { get; set; }
    }
}