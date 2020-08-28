namespace IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' of finite element methods.
    /// </summary>
    public class FiniteElementMethodInput : NumericalMethodInput
    {
        /// <summary>
        /// The mass matrix.
        /// </summary>
        public double[,] Mass { get; set; }

        /// <summary>
        /// The stiffness matrix.
        /// </summary>
        public double[,] Stiffness { get; set; }

        /// <summary>
        /// The damping matrix.
        /// </summary>
        public double[,] Damping { get; set; }

        /// <summary>
        /// The force matrix with original values.
        /// </summary>
        public double[] OriginalForce { get; set; }

        /// <summary>
        /// The number of true boundary conditions.
        /// </summary>
        public uint NumberOfTrueBoundaryConditions { get; set; }
    }
}
