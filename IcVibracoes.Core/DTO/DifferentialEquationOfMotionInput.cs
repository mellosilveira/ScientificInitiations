namespace IcVibracoes.Core.DTO
{
    /// <summary>
    /// It contains the input 'data' to create a differential equation of motion.
    /// </summary>
    public class DifferentialEquationOfMotionInput
    {
        /// <summary>
        /// Unity: kg (kilogram).
        /// Mass of primary object.
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Unity: N/m (Newton per meter).
        /// Hardness of primary object.
        /// </summary>
        public double Hardness { get; set; }

        /// <summary>
        /// Unity: kg (kilogram).
        /// Mass of secondary object.
        /// </summary>
        public double SecondaryMass { get; set; }

        /// <summary>
        /// Unity: N/m (Newton per meter).
        /// Hardness of secondary object.
        /// </summary>
        public double SecondaryHardness { get; set; }

        /// <summary>
        /// The force applied in the analyzed system.
        /// </summary>
        public double Force { get; set; }

        /// <summary>
        /// Unity: Hz (Hertz).
        /// </summary>
        public double AngularFrequency { get; set; }

        /// <summary>
        /// Don't have unit.
        /// Represents the relation between damping by critical damping.
        /// If it is equals to zero, the vibration is harmonic.
        /// If it is greather than zero and less than 1, the vibration is underdamped.
        /// If it is equals to one, the vibration is critical damped.
        /// If it is greather than 1, the vibration is overdamped.
        /// </summary>
        public double DampingRatio { get; set; }
    }
}
