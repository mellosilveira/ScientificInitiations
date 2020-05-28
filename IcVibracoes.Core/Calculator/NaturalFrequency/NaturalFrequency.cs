using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.Calculator.Eigenvalue;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.NaturalFrequency
{
    /// <summary>
    /// It's responsible to calculate the structure natural frequencies for different cases.
    /// </summary>
    public class NaturalFrequency : INaturalFrequency
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly IEigenvalue _calculateEigenvalue;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateEigenvalue"></param>
        public NaturalFrequency(
            IArrayOperation arrayOperation,
            IEigenvalue calculateEigenvalue)
        {
            this._arrayOperation = arrayOperation;
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
        public async Task<double> CalculateByInversePowerMethod(double[,] mass, double[,] stiffness, double tolerance)
        {
            double[,] inversedStiffness = await this._arrayOperation.InverseMatrix(stiffness, nameof(stiffness)).ConfigureAwait(false);

            double[,] dynamicalMatrix = await this._arrayOperation.Multiply(inversedStiffness, mass).ConfigureAwait(false);
            double[,] inversetDynamicalMatrix = await this._arrayOperation.InverseMatrix(dynamicalMatrix, nameof(dynamicalMatrix)).ConfigureAwait(false);

            double naturalFrequency = await this._calculateEigenvalue.PowerMethod(inversetDynamicalMatrix, tolerance).ConfigureAwait(false);

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
        public async Task<double[]> CalculateByQRDecomposition(double[,] mass, double[,] stiffness, double tolerance)
        {
            double[,] inversedStiffness = await this._arrayOperation.InverseMatrix(stiffness, nameof(stiffness)).ConfigureAwait(false);

            double[,] dynamicalMatrix = await this._arrayOperation.Multiply(inversedStiffness, mass).ConfigureAwait(false);

            double[] naturalFrequencies = await this._calculateEigenvalue.QR_Decomposition(dynamicalMatrix, tolerance).ConfigureAwait(false);

            return naturalFrequencies;
        }

        /// <summary>
        /// Calculates the main natural frequency of strucuture using rigid body concepts.
        /// Base equation: wn = sqrt(K / M)
        /// wn - Natural frequency.
        /// K - Structure stiffness.
        /// M - Structure mass.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public Task<double> Calculate(double mass, double stiffness)
        {
            double naturalAngularFrequency = Math.Sqrt(stiffness / mass);

            return Task.FromResult(naturalAngularFrequency);
        }
    }
}
