namespace IcVibracoes.Core.Models
{
    /// <summary>
    /// It contains the constants used in the application.
    /// </summary>
    public class Constant
    {
        /// <summary>
        /// Nodes per element in a 1D analysis.
        /// OBS.: 1D analysis just calculate the displacement in a single direction, in the case of this project, y axis.
        /// </summary>
        public const int NodesPerElement = 2;

        /// <summary>
        /// Degrees freedom per element.
        /// Calculus: Degrees freedom per element = Nodes Per Element * Degrees Freedom
        /// </summary>
        public const int DegreesFreedomElement = 4;

        /// <summary>
        /// Dimensions in the beam.
        /// OBS.: That is different than analysis dimensions.
        /// </summary>
        public const int Dimensions = 2;

        /// <summary>
        /// Degrees freedom per element.
        /// </summary>
        public const int DegreesFreedom = 2;

        /// <summary>
        /// Proportionality constant used in the damping calculate. 
        /// It is multiplied by mass matrix.
        /// </summary>
        public const double Alpha = 1e-5;

        /// <summary>
        /// Proportionality constant used in the damping calculate. 
        /// It is multiplied by mass matrix.
        /// </summary>
        public const double Mi = 0;

        /// <summary>
        /// Size of piezoelectric element size.
        /// </summary>
        public const int PiezoelectricDegreesFreedomElement = 2;

        /// <summary>
        /// Number of variables in a one degree freedom rigid body analysis.
        /// </summary>
        public const int NumberOfRigidBodyVariables_1DF = 2;

        /// <summary>
        /// Number of variables in a two degrees freedom rigid body analysis.
        /// </summary>
        public const int NumberOfRigidBodyVariables_2DF = 4;
    }
}
