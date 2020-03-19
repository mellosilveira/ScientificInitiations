using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.ArrayOperations
{
    public sealed class ArrayOperation : IArrayOperation
    {
        public Task<double[,]> AddValue(double[,] matrixToAdd, double[] values, uint[] nodePositions, string matrixName)
        {
            if (!int.Equals(values.Length, nodePositions.Length))
            {
                throw new Exception($"Error adding values in {matrixName}. The matrixes lenght {nameof(values)}: {values.Length} and {nameof(nodePositions)}: {nodePositions.Length} must be the same.");
            }

            int size = values.Length;

            for (int i = 0; i < size; i++)
            {
                matrixToAdd[2 * (nodePositions[i] - 1), 2 * (nodePositions[i] - 1)] += values[i];
            }

            return Task.FromResult(matrixToAdd);
        }

        public Task<double[]> AddValue(double[] matrixToAdd, double value, uint[] nodePositions, string matrixName)
        {
            int size = nodePositions.Length;

            for (int i = 0; i < size; i++)
            {
                matrixToAdd[2 * nodePositions[i]] += value;
            }

            return Task.FromResult(matrixToAdd);
        }

        public Task<double[]> Create(double value, uint size)
        {
            double[] newArray = new double[size];

            for (int i = 0; i < size; i++)
            {
                newArray[i] = value;
            }

            return Task.FromResult(newArray);
        }

        public Task<double[]> Create(double value, uint size, uint[] positions, string arrayName)
        {
            double[] newArray = new double[size];

            for (int i = 0; i < positions.Length; i++)
            {
                try
                {
                    newArray[positions[i] - 1] = value;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating {arrayName}. {ex.Message}.");
                }
            }

            return Task.FromResult(newArray);
        }

        public Task<double[]> Create(double[] values, uint size, uint[] positions, string arrayName)
        {
            double[] newArray = new double[size];

            for (int i = 0; i < positions.Length; i++)
            {
                try
                {
                    newArray[positions[i] - 1] = values[i];
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating {arrayName}. {ex.Message}.");
                }
            }

            return Task.FromResult(newArray);
        }

        public Task<double[,]> InverseMatrix(double[,] matrix, string matrixName)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
            {
                throw new Exception($"Error inversing {matrixName}. It is just possible to inverse a quadratic matrix.");
            }

            int n = matrix.GetLength(0);
            double[,] matrizInv = new double[n, n];
            double pivot, p;
            int i, j, k, l;

            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrizInv[i, j] = 1;
                    }
                    else
                    {
                        matrizInv[i, j] = 0;
                    }
                }
            }

            // Triangularization
            for (i = 0; i < n; i++)
            {
                pivot = matrix[i, i];

                for (l = 0; l < n; l++)
                {
                    matrix[i, l] = matrix[i, l] / pivot;
                    matrizInv[i, l] = matrizInv[i, l] / pivot;
                }

                for (k = i + 1; k < n; k++)
                {
                    p = matrix[k, i];

                    for (j = 0; j < n; j++)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        matrizInv[k, j] = matrizInv[k, j] - (p * matrizInv[i, j]);
                    }
                }
            }

            // Retrosubstitution
            for (i = n - 1; i >= 0; i--)
            {
                for (k = i - 1; k >= 0; k--)
                {
                    p = matrix[k, i];

                    for (j = n - 1; j >= 0; j--)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        matrizInv[k, j] = matrizInv[k, j] - (p * matrizInv[i, j]);
                    }
                }
            }

            return Task.FromResult(matrizInv);
        }

        public Task<double[,]> InverseMatrix(double[,] matrix, uint size, string matrixName)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
            {
                throw new Exception($"Error inversing {matrixName}. It is just possible to inverse a quadratic matrix. Sizes: {matrix.GetLength(0)}, {matrix.GetLength(1)}.");
            }

            if (matrix.GetLength(0) < size)
            {
                throw new Exception($"Error inversing {matrixName}. The size passed: {size} can't be bigger than the matrix sizes: {matrix.GetLength(0)}, {matrix.GetLength(1)}.");
            }

            int n = matrix.GetLength(0);
            double[,] inversedMatrix = new double[n, n];
            double pivot, p;
            int i, j, k, l;

            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        inversedMatrix[i, j] = 1;
                    }
                    else
                    {
                        inversedMatrix[i, j] = 0;
                    }
                }
            }

            // Triangularization
            for (i = 0; i < n; i++)
            {
                pivot = matrix[i, i];

                if(pivot == 0)
                {
                    throw new Exception($"Pivot can't be zero. Position: row = {i}, column = {i}.");
                }

                for (l = 0; l < n; l++)
                {
                    matrix[i, l] = matrix[i, l] / pivot;
                    inversedMatrix[i, l] = inversedMatrix[i, l] / pivot;
                }

                for (k = i + 1; k < n; k++)
                {
                    p = matrix[k, i];

                    for (j = 0; j < n; j++)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        inversedMatrix[k, j] = inversedMatrix[k, j] - (p * inversedMatrix[i, j]);
                    }
                }
            }

            // Retrosubstitution
            for (i = n - 1; i >= 0; i--)
            {
                for (k = i - 1; k >= 0; k--)
                {
                    p = matrix[k, i];

                    for (j = n - 1; j >= 0; j--)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        inversedMatrix[k, j] = inversedMatrix[k, j] - (p * inversedMatrix[i, j]);
                    }
                }
            }

            double[,] result = new double[size, size];
            for (i = 0; i < size; i++)
            {
                for (j = 0; j < size; j++)
                {
                    result[i, j] = inversedMatrix[i, j];
                }
            }

            return Task.FromResult(result);
        }

        public Task<double[]> MergeArray(double[] array1, double[] array2)
        {
            int size = array1.Length + array2.Length;
            double[] mergedArray = new double[size];

            for (int i = 0; i < size; i++)
            {
                if (i < array1.Length)
                {
                    mergedArray[i] = array1[i];
                }
                else
                {
                    mergedArray[i] = array2[i - array1.Length];
                }
            }

            return Task.FromResult(mergedArray);
        }

        public Task<bool[]> MergeArray(bool[] array1, bool[] array2)
        {
            int size = array1.Length + array2.Length;
            bool[] mergedArray = new bool[size];

            for (int i = 0; i < size; i++)
            {
                if (i < array1.Length)
                {
                    mergedArray[i] = array1[i];
                }
                else
                {
                    mergedArray[i] = array2[i - array1.Length];
                }
            }

            return Task.FromResult(mergedArray);
        }

        public Task<double[,]> Multiply(double[,] array1, double[,] array2, string matrixesName)
        {
            int rows1 = array1.GetLength(0);
            int columns1 = array1.GetLength(1);
            int rows2 = array2.GetLength(0);
            int columns2 = array2.GetLength(1);

            if (columns1 != rows2)
            {
                throw new Exception($"Error multiplying {matrixesName}. Number of columns in matrix1: {columns1} have to be the same of the number of rows in matrix2: {rows2}.");
            }

            double[,] arrayMultiplication = new double[rows1, columns2];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < columns1; j++)
                {
                    double sum = 0;

                    for (int k = 0; k < columns1; k++)
                    {
                        sum += array1[i, k] * array2[k, j];
                    }

                    arrayMultiplication[i, j] = sum;
                }
            }

            return Task.FromResult(arrayMultiplication);
        }

        public Task<double[]> Multiply(double[,] array1, double[] array2, string matrixesName)
        {
            int rows1 = array1.GetLength(0);
            int columns1 = array1.GetLength(1);
            int size2 = array2.Length;

            if (columns1 != size2)
            {
                throw new Exception($"Error multiplying {matrixesName}. Number of columns in matrix1: {columns1} have to be the same of the array2 size: {size2}.");
            }

            double[] arrayMultiplication = new double[rows1];

            for (int i = 0; i < rows1; i++)
            {
                double sum = 0;

                for (int j = 0; j < columns1; j++)
                {
                    sum += array1[i, j] * array2[j];
                }

                arrayMultiplication[i] = sum;
            }

            return Task.FromResult(arrayMultiplication);
        }

        public Task<double[]> Multiply(double[] array1, double[,] array2, string matrixesName)
        {
            int size1 = array1.Length;
            int rows2 = array2.GetLength(0);
            int columns2 = array2.GetLength(1);

            if (size1 != rows2)
            {
                throw new Exception($"Error multiplying {matrixesName}. Array1 size: {size1} have to be the same of the Number of rows in matrix2: {rows2}.");
            }

            double[] arrayMultiplication = new double[rows2];

            for (int i = 0; i < columns2; i++)
            {
                double sum = 0;

                for (int j = 0; j < size1; j++)
                {
                    sum += array1[j] * array2[j, i];
                }

                arrayMultiplication[i] = sum;
            }

            return Task.FromResult(arrayMultiplication);
        }

        public Task<double[,]> Subtract(double[,] array1, double[,] array2, string matrixesName)
        {
            int rows1 = array1.GetLength(0);
            int columns1 = array1.GetLength(1);
            int rows2 = array2.GetLength(0);
            int columns2 = array2.GetLength(1);

            if (rows1 != rows2 || columns1 != columns2)
            {
                throw new Exception($"Can't subtract matrixes: {matrixesName}. The sizes is differents. Matrix1: {rows1}, {columns1}. Matrix2: {rows2}, {columns2}.");
            }

            double[,] arraySubtraction = new double[rows1, columns1];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < columns1; j++)
                {
                    arraySubtraction[i, j] = array1[i, j] - array2[i, j];
                }
            }

            return Task.FromResult(arraySubtraction);
        }

        public Task<double[]> Subtract(double[] array1, double[] array2, string arraysName)
        {
            int size1 = array1.Length;
            int size2 = array2.Length;

            if (size1 != size2)
            {
                throw new Exception($"Can't subtract arrays: {arraysName}. The sizes is differents. Array1: {size1}. Array2: {size2}.");
            }

            double[] arraySubtraction = new double[size1];

            for (int i = 0; i < array1.Length; i++)
            {
                arraySubtraction[i] = array1[i] - array2[i];
            }

            return Task.FromResult(arraySubtraction);
        }

        public Task<double[]> Subtract(double[] arrayToSubtract, double[] array2, double[] array3, string arraysName)
        {
            int size1 = arrayToSubtract.Length;
            int size2 = array2.Length;
            int size3 = array3.Length;

            if (size1 != size2 || size1 != size3 || size2 != size3)
            {
                throw new Exception($"Can't subtract arrays: {arraysName}. The sizes is differents. {nameof(arrayToSubtract)}: {size1}. {nameof(array2)}: {size2}. {nameof(array3)}: {size3}.");
            }

            double[] arraySubtraction = new double[size1];

            for (int i = 0; i < arrayToSubtract.Length; i++)
            {
                arraySubtraction[i] = arrayToSubtract[i] - array2[i] - array3[i];
            }

            return Task.FromResult(arraySubtraction);
        }

        public Task<double[,]> Sum(double[,] matrix1, double[,] matrix2, string matrixesName)
        {
            int rows1 = matrix1.GetLength(0);
            int columns1 = matrix1.GetLength(1);
            int rows2 = matrix2.GetLength(0);
            int columns2 = matrix2.GetLength(1);

            if (rows1 != rows2 || columns1 != columns2)
            {
                throw new Exception($"Can't sum matrixes: {matrixesName}. The sizes is differents. Matrix1: {rows1}, {columns1}. Matrix2: {rows2}, {columns2}.");
            }

            double[,] arraySum = new double[rows1, columns1];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < columns1; j++)
                {
                    arraySum[i, j] = matrix1[i, j] + matrix2[i, j];
                }
            }

            return Task.FromResult(arraySum);
        }

        public Task<double[]> Sum(double[] array1, double[] array2, double[] array3, string matrixesName)
        {
            int size1 = array1.GetLength(0);
            int size2 = array2.GetLength(0);
            int size3 = array3.GetLength(0);

            if (size1 != size2 || size1 != size3 || size2 != size3)
            {
                throw new Exception($"Can't sum matrixes: {matrixesName}. The sizes is differents. {nameof(array1)}: {size1}. {nameof(array2)}: {size2}. {nameof(array3)}: {size3}.");
            }

            double[] arraySum = new double[size1];

            for (int i = 0; i < size1; i++)
            {
                arraySum[i] = array1[i] + array2[i] + array3[i];
            }

            return Task.FromResult(arraySum);
        }

        public Task<double[]> Sum(double[] array1, double[] array2, string arraysName)
        {
            int size1 = array1.Length;
            int size2 = array2.Length;

            if (size1 != size2)
            {
                throw new Exception($"Can't sum arrays: {arraysName}. The sizes is differents. Array1: {size1}. Array2: {size2}.");
            }

            double[] arraySum = new double[size1];

            for (int i = 0; i < size1; i++)
            {
                arraySum[i] = array1[i] + array2[i];
            }

            return Task.FromResult(arraySum);
        }

        public Task<double[,]> TransposeMatrix(double[,] matrix)
        {
            int row = matrix.GetLength(0);
            int column = matrix.GetLength(1);
            double[,] matrixTransposed = new double[column, row];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    matrixTransposed[j, i] = matrix[i, j];
                }
            }

            return Task.FromResult(matrixTransposed);
        }
    }
}
