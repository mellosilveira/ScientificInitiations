using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.Eigenvalue
{
    /// <summary>
    /// It's responsible to calculate the eigenvalues to a matrix.
    /// It's used in natural frequency calculation.
    /// </summary>
    public interface IEigenvalue
    {
        /// <summary>
        /// /// Calculates the biggest eigenvalue using Power Method.
        /// Equations to be used:
        /// [zk+1] = [Matrix] x [yk].
        /// alphak = max([zk])
        /// [yk] = [zk] / alphak
        /// [eigenvalue] = [zk+1] / [yk].
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        Task<double> PowerMethod(double[,] matrix, double tolerance);

        /// <summary>
        /// Calculates the eigenvalues of a matrix using QR Decomposition.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        Task<double[]> QR_Decomposition(double[,] matrix, double tolerance);
    }
}