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
        /// Damping ratio represents the relation between damping by critical damping.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// </summary>
        public List<double> DampingRatioList { get; set; }

        /// <summary>
        /// The time-step to be used in the numerical integration method.
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// The initial time in the analysis.
        /// </summary>
        public double InitialTime { get; set; }

        /// <summary>
        /// The final time in the analysis.
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// Initial angular frequency of the analysis.
        /// </summary>
        public double InitialAngularFrequency { get; set; }

        /// <summary>
        /// Angular frequency step of the analysis.
        /// </summary>
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// Final angular frequency of the analysis.
        /// </summary>
        public double FinalAngularFrequency { get; set; }

        /// <summary>
        /// The force applied in the main object.
        /// </summary>
        public double Force { get; set; }
    }
}
