using IcVibracoes.Core.Calculator.Eigenvalue;
using IcVibracoes.Core.ExtensionMethods;
using System;

namespace IcVibracoes.Core.Calculator.NaturalFrequency
{
    /// <summary>
    /// It's responsible to calculate the structure natural frequencies for different cases.
    /// </summary>
    public class NaturalFrequency : INaturalFrequency
    {
        private readonly IEigenvalue _calculateEigenvalue;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="calculateEigenvalue"></param>
        public NaturalFrequency(IEigenvalue calculateEigenvalue)
        {
            this._calculateEigenvalue = calculateEigenvalue;
        }

        /// <summary>
        /// Calculates the max value to natural frequency of structure using finite element concepts.
        /// Base equation: det([K] - wn²[M]) = 0 
        /// Equation to be used in methods: det((1/wn²)[I] - inv([K])[M])
        /// wn - Natural frequency.
        /// [K] - Structure stiffness matrix.
        /// [M] - Structure mass matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public double CalculateByInversePowerMethod(double[,] mass, double[,] stiffness, double tolerance)
        {
            double[,] inversedStiffness = stiffness.InverseMatrix();

            double[,] dynamicalMatrix = inversedStiffness.Multiply(mass);
            double[,] inversetDynamicalMatrix = dynamicalMatrix.InverseMatrix();

            double naturalFrequency = this._calculateEigenvalue.PowerMethod(inversetDynamicalMatrix, tolerance);

            return naturalFrequency;
        }

        /// <summary>
        /// Calculates the main natural frequencies of structure using finite element concepts.
        /// Base equation: det([K] - wn²[M]) = 0
        /// wn - Natural frequency.
        /// [K] - Structure stiffness matrix.
        /// [M] - Structure mass matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public double[] CalculateByQRDecomposition(double[,] mass, double[,] stiffness, double tolerance)
        {
            double[,] inversedStiffness = stiffness.InverseMatrix();

            double[,] dynamicalMatrix = inversedStiffness.Multiply(mass);

            double[] naturalFrequencies = this._calculateEigenvalue.QR_Decomposition(dynamicalMatrix, tolerance);

            return naturalFrequencies;
        }

        /// <summary>
        /// Calculates the main natural frequency of structure using rigid body concepts.
        /// Base equation: wn = sqrt(K / M)
        /// wn - Natural frequency.
        /// K - Structure stiffness.
        /// M - Structure mass.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public double Calculate(double mass, double stiffness) => Math.Sqrt(stiffness / mass);
    }
}
