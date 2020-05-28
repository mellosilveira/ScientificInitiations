using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.NaturalFrequency
{
    /// <summary>
    /// It's responsible to calculate the structure natural frequencies for different cases.
    /// </summary>
    public interface INaturalFrequency
    {
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
        Task<double> Calculate(double mass, double stiffness);

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
        Task<double> CalculateByInversePowerMethod(double[,] mass, double[,] stiffness, double tolerance);

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
        Task<double[]> CalculateByQRDecomposition(double[,] mass, double[,] stiffness, double tolerance);
    }
}
