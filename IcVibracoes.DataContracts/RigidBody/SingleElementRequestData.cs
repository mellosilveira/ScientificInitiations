using IcVibracoes.Common.Classes;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It represents the 'data' content of a single element to be used in RigidBody operations.
    /// </summary>
    public class SingleElementRequestData
    {
        /// <summary>
        /// The mechanical properties of the element that will be analyzed.
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
