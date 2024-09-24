﻿namespace IcVibracoes.Core.Models
{
    /// <summary>
    /// It contains the constants used in the application.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Nodes per element in a 1D analysis.
        /// OBS.: 1D analysis just calculate the displacement in a single direction, in the case of this project, y axis.
        /// </summary>
        public const byte NodesPerElement = 2;

        /// <summary>
        /// Degrees freedom per per.
        /// </summary>
        public const byte DegreesOfFreedomPerNode = 2;

        /// <summary>
        /// Degrees freedom per element.
        /// Degrees freedom per element = Nodes Per Element * Degrees of Freedom
        /// </summary>
        public const byte DegreesOfFreedomElement = 4;

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
        /// Piezoelectric degrees of freedom per element.
        /// </summary>
        public const byte PiezoelectricDegreesOfFreedomElement = 2;

        /// <summary>
        /// Number of variables in a one degree freedom rigid body analysis.
        /// </summary>
        public const byte NumberOfRigidBodyVariables1Df = 3;

        /// <summary>
        /// Number of variables in a two degrees freedom rigid body analysis.
        /// </summary>
        public const byte NumberOfRigidBodyVariables2Df = 6;
    }
}
