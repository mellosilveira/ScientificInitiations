using IcVibracoes.Core.Models.BeamCharacteristics;
using System;

namespace IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody
{
    /// <summary>
    /// It contains the input 'data' to rigid body numerical methods.
    /// </summary>
    public class RigidBodyInput : NumericalMethodInput
    {
        /// <summary>
        /// Mass of primary object.
        /// Unity: kg (kilogram).
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Stiffness of primary object.
        /// Unity: N/m (Newton per meter).
        /// </summary>
        public double Stiffness { get; set; }

        /// <summary>
        /// Represents the relation between damping by critical damping.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// Unit: dimensionless.
        /// </summary>
        public double DampingRatio { get; set; }

        /// <summary>
        /// Damping of primary object.
        /// Unity: Ns/m (Newton-second per meter).
        /// </summary>
        public double Damping
        {
            get
            {
                return this.DampingRatio * 2 * this.Mass * Math.Sqrt(this.Stiffness / this.Mass);
            }
        }

        /// <summary>
        /// The force applied in the analyzed system.
        /// Unit: N (Newton).
        /// </summary>
        public double Force { get; set; }

        /// <summary>
        /// The type of the force.
        /// Can be harmonic or impact.
        /// </summary>
        public ForceType ForceType { get; set; }
    }
}
