using IcVibracoes.Core.AuxiliarOperations.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.Eigenvalue;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.NaturalFrequency
{
    public class NaturalFrequency : INaturalFrequency
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly ICalculateEigenvalue _calculateEigenvalue;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateEigenvalue"></param>
        public NaturalFrequency(
            IArrayOperation arrayOperation,
            ICalculateEigenvalue calculateEigenvalue)
        {
            this._arrayOperation = arrayOperation;
            this._calculateEigenvalue = calculateEigenvalue;
        }

        /// <summary>
        /// Calculates the max value to natural frequency of structure using finite element concepts.
        /// Base equation: det([K] - w²[M]) = 0 
        /// Equation to be used in methods: det((1/w²)[I] - inv([K])[M])
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

        public async Task<double[]> CalculateByQRDecomposition(double[,] mass, double[,] stiffness, double tolerance)
        {
            double[,] inversedStiffness = await this._arrayOperation.InverseMatrix(stiffness, nameof(stiffness)).ConfigureAwait(false);

            double[,] dynamicalMatrix = await this._arrayOperation.Multiply(inversedStiffness, mass).ConfigureAwait(false);

            double[] naturalFrequencies = await this._calculateEigenvalue.QR_Decomposition(dynamicalMatrix, tolerance).ConfigureAwait(false);

            return naturalFrequencies;
        }

        public Task<double> Calculate(double mass, double stiffness)
        {
            double naturalAngularFrequency = Math.Sqrt(stiffness / mass);

            return Task.FromResult(naturalAngularFrequency);
        }
    }
}
