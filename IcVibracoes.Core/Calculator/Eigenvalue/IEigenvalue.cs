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
        /// Logic used:
        ///     https://sites.icmc.usp.br/andretta/ensino/aulas/sme0301-1-10/AutovaloresFrancis.pdf
        ///     https://pt.wikipedia.org/wiki/Decomposi%C3%A7%C3%A3o_QR
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        Task<double[]> QR_Decomposition(double[,] matrix, double tolerance);
    }
}