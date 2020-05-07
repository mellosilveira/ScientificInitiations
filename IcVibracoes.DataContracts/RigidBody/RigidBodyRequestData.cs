using System.Collections.Generic;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It represents the 'data' content of Rigid Body request operation.
    /// </summary>
    public class RigidBodyRequestData : OperationRequestData
    {
        /// <summary>
        /// List of damping ratio of system.
        /// For each value in the list, it is made a new analysis.
        /// Damping ratio represents the relation between damping by critical damping.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// </summary>
        public List<double> DampingRatioList { get; set; }

        /// <summary>
        /// The force applied in the main object.
        /// </summary>
        public double Force { get; set; }
    }
}
