using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.ArrayOperations
{
    public interface IArrayOperation
    {
        Task<double[]> MergeArray(double[] array1, double[] array2);

        Task<bool[]> MergeArray(bool[] array1, bool[] array2);

        Task<double[,]> InverseMatrix(double[,] matrix, string matrixName);

        Task<double[,]> InverseMatrix(double[,] matrix, uint size, string matrixName);

        Task<double[,]> Multiply(double[,] matrix1, double[,] matrix2, string matrixesName);

        Task<double[]> Multiply(double[,] matrix, double[] array, string arraysName);

        Task<double[]> Multiply(double[] array, double[,] matrix, string arraysName);

        Task<double[,]> Subtract(double[,] matrix1, double[,] matrix2, string matrixesName);

        Task<double[]> Subtract(double[] array1, double[] array2, string arraysName);

        Task<double[]> Subtract(double[] array1, double[] array2, double[] array3, string arraysName);

        Task<double[,]> Sum(double[,] matrix1, double[,] matrix2, string matrixesName);

        Task<double[]> Sum(double[] array1, double[] array2, double[] array3, string matrixesName);

        Task<double[]> Sum(double[] array1, double[] array2, string arraysName);

        Task<double[]> Create(double value, uint size);

        Task<double[]> Create(double value, uint size, uint[] positions, string arrayName);

        Task<double[]> Create(double[] values, uint size, uint[] positions, string arrayName);

        Task<double[,]> TransposeMatrix(double[,] matrix);

        Task<double[,]> AddValue(double[,] matrixToAdd, double[] values, uint[] valueNodePositions, string matrixName);

        // Está errado. É para criar um array com valores em alguns pontos.
        Task<double[]> AddValue(double[] matrixToAdd, double value, uint[] valueNodePositions, string matrixName);
    }
}
