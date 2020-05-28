using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.NaturalFrequency
{
    public interface INaturalFrequency
    {
        Task<double> Calculate(double mass, double stiffness);

        Task<double> CalculateByInversePowerMethod(double[,] mass, double[,] stiffness, double tolerance);

        Task<double[]> CalculateByQRDecomposition(double[,] mass, double[,] stiffness, double tolerance);
    }
}
