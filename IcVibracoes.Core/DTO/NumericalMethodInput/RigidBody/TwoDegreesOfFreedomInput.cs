namespace IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody
{
    /// <summary>
    /// It contains the input 'data' to create a differential equation of motion for two degrees of freedom analysis.
    /// </summary>
    public class TwoDegreesOfFreedomInput : RigidBodyInput
    {
        /// <summary>
        /// Mass of secondary object.
        /// Unity: kg (kilogram).
        /// </summary>
        public double SecondaryMass { get; set; }

        /// <summary>
        /// Stiffness of secondary object.
        /// Unity: N/m (Newton per meter).
        /// </summary>
        public double SecondaryStiffness { get; set; }
    }
}
