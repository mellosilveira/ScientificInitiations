using IcVibracoes.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.ArrayOperations
{
    /// <summary>
    /// It contains the methods to execute every vector operations.
    /// </summary>
    public sealed class ArrayOperation : IArrayOperation
    {
        /// <summary>
        /// It's responsible to add values in a matrix passing the node positions of each value.
        /// </summary>
        /// <param name="matrixToAdd"></param>
        /// <param name="values"></param>
        /// <param name="nodePositions"></param>
        /// <param name="matrixName"></param>
        /// <returns></returns>
        public Task<double[,]> AddValue(double[,] matrixToAdd, double[] values, uint[] nodePositions, string matrixName)
        {
            if (!int.Equals(values.Length, nodePositions.Length))
            {
                throw new ArgumentException($"Error adding values in {matrixName}. The matrixes lenght {nameof(values)}: {values.Length} and {nameof(nodePositions)}: {nodePositions.Length} must be the same.");
            }

            int size = values.Length;

            // Parallel.For
            for (int i = 0; i < size; i++)
            {
                try
                {
                    matrixToAdd[2 * nodePositions[i], 2 * nodePositions[i]] += values[i];
                }
                catch
                {
                    throw new ArgumentOutOfRangeException($"It was not possible to add the value: {values[i]} in matrix: {matrixName} at the position: {i}.");
                }
            }

            return Task.FromResult(matrixToAdd);
        }

        /// <summary>
        /// It's responsible to calculate the inner product between two matrixes.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public Task<double> CalculateInnerProduct(double[] vector1, double[] vector2)
        {
            double result = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// It's responsible to create a matrix with a unique value in all positions with a size that is informed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="vectorName"></param>
        /// <returns></returns>
        public Task<double[]> CreateVector(double value, uint size, string vectorName)
        {
            if (size < 1)
            {
                throw new ArgumentException($"Cannot create a matrix with size: {size}. It must be greather or equals to 1.");
            }

            double[] newVector = new double[size];

            // Parallel.For
            for (int i = 0; i < size; i++)
            {
                newVector[i] = value;
            }

            return Task.FromResult(newVector);
        }

        /// <summary>
        /// It's responsible to create a vector with an unique value in the informed positions with a size that is informed too.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="elementPositions"></param>
        /// <param name="vectorName"></param>
        /// <returns></returns>
        public Task<double[]> CreateVector(double value, uint size, uint[] elementPositions, string vectorName)
        {
            if (size < 1)
            {
                throw new ArgumentException($"Cannot create a matrix with size: {size}. It must be greather or equals to 1.");
            }

            double[] newVector = new double[size];

            try
            {
                for (int i = 0; i < elementPositions.Length; i++)
                {
                    newVector[elementPositions[i] - 1] = value;
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException($"Error creating the vector: {vectorName}.");
            }

            return Task.FromResult(newVector);
        }

        /// <summary>
        /// It's responsible to inverse a matrix using the Gauss-Jordan method.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="matrixName"></param>
        /// <returns></returns>
        public Task<double[,]> InverseMatrix(double[,] matrix, string matrixName)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
            {
                throw new ArgumentException($"Error inversing {matrixName}. It is just possible to inverse a square matrix.");
            }

            int n = matrix.GetLength(0);
            double[,] matrizInv = new double[n, n];
            double pivot, p;

            // Parallel.For
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
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
            for (int i = 0; i < n; i++)
            {
                pivot = matrix[i, i];
                if (pivot == 0)
                {
                    throw new DivideByZeroException($"Invalid value to pivot: {pivot}.");
                }

                // Parallel.For
                for (int l = 0; l < n; l++)
                {
                    matrix[i, l] = matrix[i, l] / pivot;
                    matrizInv[i, l] = matrizInv[i, l] / pivot;
                }

                for (int k = i + 1; k < n; k++)
                {
                    p = matrix[k, i];

                    // Parallel.For
                    for (int j = 0; j < n; j++)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        matrizInv[k, j] = matrizInv[k, j] - (p * matrizInv[i, j]);
                    }
                }
            }

            // Retrosubstitution
            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = i - 1; k >= 0; k--)
                {
                    p = matrix[k, i];

                    // Parallel.For
                    for (int j = n - 1; j >= 0; j--)
                    {
                        matrix[k, j] = matrix[k, j] - (p * matrix[i, j]);
                        matrizInv[k, j] = matrizInv[k, j] - (p * matrizInv[i, j]);
                    }
                }
            }

            return Task.FromResult(matrizInv);
        }

        /// <summary>
        /// It's responsible to merge two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public Task<T[]> MergeVectors<T>(T[] vector1, T[] vector2)
        {
            int vector1Length = vector1.Length;
            int size = vector1Length + vector2.Length;
            T[] mergedVector = new T[size];

            for (int i = 0; i < size; i++)
            {
                if (i < vector1Length)
                {
                    mergedVector[i] = vector1[i];
                }
                else
                {
                    mergedVector[i] = vector2[i - vector1Length];
                }
            }

            return Task.FromResult(mergedVector);
        }

        /// <summary>
        /// It's responsible to multiplicate a matrix and a vector.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <param name="arraysName"></param>
        /// <returns></returns>
        public Task<double[]> Multiply(double[,] matrix, double[] vector, string arraysName)
        {
            int rows1 = matrix.GetLength(0);
            int columns1 = matrix.GetLength(1);
            int size2 = vector.Length;

            if (columns1 != size2)
            {
                throw new Exception($"Error multiplying {arraysName}. Number of columns in matrix1: {columns1} have to be the same of the vector2 size: {size2}.");
            }

            double[] vectorMultiplication = new double[rows1];

            for (int i = 0; i < rows1; i++)
            {
                double sum = 0;

                for (int j = 0; j < columns1; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }

                vectorMultiplication[i] = sum;
            }

            return Task.FromResult(vectorMultiplication);
        }

        /// <summary>
        /// It's responsible to multiplicate two matrixes.
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        public Task<double[,]> Multiply(double[,] matrix1, double[,] matrix2)
        {
            int rows1 = matrix1.GetLength(0);
            int columns2 = matrix2.GetLength(1);

            double[,] result = new double[rows1, columns2];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < columns2; j++)
                {
                    double sum = 0;

                    for (int k = 0; k < rows1; k++)
                    {
                        sum += matrix1[i, k] * matrix2[k, j];
                    }

                    result[i, j] = sum;
                }
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// It's responsible to subtract two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public Task<double[]> Subtract(double[] vector1, double[] vector2)
        {
            int size = vector1.Length;

            double[] result = new double[size];

            for (int i = 0; i < size; i++)
            {
                result[i] = vector1[i] - vector2[i];
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// It's responsible to sum three vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <param name="vectorsName"></param>
        /// <returns></returns>
        public Task<double[]> Sum(double[] vector1, double[] vector2, double[] vector3, string vectorsName)
        {
            int size1 = vector1.GetLength(0);
            int size2 = vector2.GetLength(0);
            int size3 = vector3.GetLength(0);

            if (size1 != size2 || size1 != size3 || size2 != size3)
            {
                throw new Exception($"Can't sum matrixes: {vectorsName}. The sizes is differents. Vector1: {size1}. Vector2: {size2}. Vector3: {size3}.");
            }

            double[] vectorSum = new double[size1];

            for (int i = 0; i < size1; i++)
            {
                vectorSum[i] = vector1[i] + vector2[i] + vector3[i];
            }

            return Task.FromResult(vectorSum);
        }

        /// <summary>
        /// It1s responsible to sum two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public Task<double[]> Sum(double[] vector1, double[] vector2)
        {
            int size = vector1.Length;

            double[] result = new double[size];

            for (int i = 0; i < size; i++)
            {
                result[i] = vector1[i] + vector2[i];
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// It's responsible to calculate the transposed matrix of a informed matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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
