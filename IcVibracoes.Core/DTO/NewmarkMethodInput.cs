using IcVibracoes.Common.Classes;

namespace IcVibracoes.Core.DTO.Input
{
    /// <summary>
    /// It contains the input content of newmark method operations.
    /// </summary>
    public class NewmarkMethodInput
    {
        /// <summary>
        /// Mass matrix of the object that is analyzed.
        /// </summary>
        public double[,] Mass { get; set; }

        /// <summary>
        /// Stiffness matrix of the object that is analyzed.
        /// </summary>
        public double[,] Stiffness { get; set; }

        /// <summary>
        /// Damping matrix of the object that is analyzed.
        /// </summary>
        public double[,] Damping { get; set; }

        /// <summary>
        /// Force vector of the object that is analyzed.
        /// </summary>
        public double[] Force { get; set; }

        /// <summary>
        /// Original force vector applied in the object.
        /// </summary>
        public double[] OriginalForce { get; set; }

        /// <summary>
        /// Time step.
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Angular frequency used in the analysis.
        /// </summary>
        public double AngularFrequency { get; set; }

        /// <summary>
        /// Number of boundary conditions that is true.
        /// </summary>
        public uint NumberOfTrueBoundaryConditions { get; set; }

        /// <summary>
        /// Newmark method parameters.
        /// </summary>
        public NewmarkMethodParameter Parameter { get; set; }
    }
}
