using System.Collections.Generic;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It represents the 'data' content of Rigid Body request operation.
    /// </summary>
    public class RigidBodyRequestData
    {
        /// <summary>
        /// List of damping ratio of system.
        /// For each value in the list, it is made a new analysis.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// </summary>
        public List<double> DampingRatio { get; set; }

        /// <summary>
        /// Initial displacement to the analysis.
        /// </summary>
        public double InitialDisplacement { get; set; }

        /// <summary>
        /// Initial velocity to the analysis.
        /// </summary>
        public double InitialVelocity { get; set; }
    }
}
