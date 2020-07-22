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
        public const byte NodesPerElement = 2;

        /// <summary>
        /// Degrees freedom per element.
        /// Calculus: Degrees freedom per element = Nodes Per Element * Degrees Freedom
        /// </summary>
        public const byte DegreesOfFreedomElement = 4;

        /// <summary>
        /// Dimensions in the beam.
        /// OBS.: That is different than analysis dimensions.
        /// </summary>
        public const byte Dimensions = 2;

        /// <summary>
        /// Degrees freedom per element.
        /// </summary>
        public const byte DegreesOfFreedom = 2;

        /// <summary>
        /// Proportionality constant used in the damping calculate. 
        /// It multiplies the stiffness matrix.
        /// </summary>
        public const double Alpha = 1e-5;

        /// <summary>
        /// Proportionality constant used in the damping calculate. 
        /// It multiplies the mass matrix.
        /// </summary>
        public const double Mi = 0;

        /// <summary>
        /// Size of piezoelectric element size.
        /// </summary>
        public const byte PiezoelectricDegreesOfFreedomElement = 2;

        /// <summary>
        /// Number of variables in a one degree freedom rigid body analysis.
        /// </summary>
        public const byte NumberOfRigidBodyVariables_1DF = 2;

        /// <summary>
        /// Number of variables in a two degrees freedom rigid body analysis.
        /// </summary>
        public const byte NumberOfRigidBodyVariables_2DF = 4;
    }
}
