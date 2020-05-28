namespace IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody
{
    /// <summary>
    /// It contains the input 'data' to create a differential equation of motion for one degree of freedom analysis.
    /// </summary>
    public class OneDegreeOfFreedomInput : RigidBodyInput
    {
        /// <summary>
        /// Mass of object.
        /// Unity: kg (kilogram).
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Stiffness of object.
        /// Unity: N/m (Newton per meter).
        /// </summary>
        public double Stiffness { get; set; }
    }
}
