using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the request content to Rigid Body operation.
    /// </summary>
    public abstract class RigidBodyRequest : OperationRequestBase
    {
        /// <summary>
        /// List of damping ratio of system.
        /// For each value in the list, it is made a new analysis.
        /// Damping ratio represents the relation between damping by critical damping.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// Unit: Dimensionless.
        /// </summary>
        /// <example>0.05</example>
        [Required]
        public ICollection<double> DampingRatios { get; set; }

        /// <summary>
        /// The force applied in the main object.
        /// Unit: N (Newton)
        /// </summary>
        /// <example>10</example>
        [Required]
        public double Force { get; set; }
    }
}
