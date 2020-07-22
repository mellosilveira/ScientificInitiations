using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.ExtensionMethods
{
    /// <summary>
    /// It contains extension methods to array.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// This method adds values in a matrix informing the node positions of each value.
        /// </summary>
        /// <param name="matrixToAdd"></param>
        /// <param name="values"></param>
        /// <param name="nodePositions"></param>
        /// <returns></returns>
        public static Task AddPerNodePositionAsync(this double[,] matrixToAdd, double[] values, uint[] nodePositions)
        {
            int size = values.Length;

            for (int i = 0; i < size; i++)
            {
                matrixToAdd[2 * nodePositions[i], 2 * nodePositions[i]] += values[i];
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// This method calculates the inner product between two matrixes.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>The inner product between two matrixes.</returns>
        public static Task<double> CalculateInnerProductAsync(this double[] vector1, double[] vector2)
        {
            double result = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// This method inverses a matrix using the Gauss-Jordan method.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="matrixName"></param>
        /// <returns>The inversed matrix using the Gauss-Jordan method.</returns>
        public static Task<double[,]> InverseMatrixAsync(this double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] matrizInv = new double[n, n];
            double pivot, p;

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
                    throw new DivideByZeroException($"Pivot cannot be zero at line {i}.");
                }

                for (int l = 0; l < n; l++)
                {
                    matrix[i, l] = matrix[i, l] / pivot;
                    matrizInv[i, l] = matrizInv[i, l] / pivot;
                }

                for (int k = i + 1; k < n; k++)
                {
                    p = matrix[k, i];

                    for (int j = 0; j < n; j++)
                    {
                        matrix[k, j] = matrix[k, j] - p * matrix[i, j];
                        matrizInv[k, j] = matrizInv[k, j] - p * matrizInv[i, j];
                    }
                }
            }

            // Retrosubstitution
            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = i - 1; k >= 0; k--)
                {
                    p = matrix[k, i];

                    for (int j = n - 1; j >= 0; j--)
                    {
                        matrix[k, j] = matrix[k, j] - p * matrix[i, j];
                        matrizInv[k, j] = matrizInv[k, j] - p * matrizInv[i, j];
                    }
                }
            }

            return Task.FromResult(matrizInv);
        }

        /// <summary>
        /// This method combines two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new array with the values combined of others two vectors.</returns>
        public static Task<T[]> CombineVectorsAsync<T>(this T[] vector1, T[] vector2)
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
        /// This method multiplicates a matrix and a vector.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns>A new vector with the result of multiplication between a matrix and a vector.</returns>
        public static Task<double[]> MultiplyAsync(this double[,] matrix, double[] vector)
        {
            int rows1 = matrix.GetLength(0);
            int columns1 = matrix.GetLength(1);

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
        /// This method multiplicates two matrixes.
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns>A new matrix with the result of multiplication between two matrixes.</returns>
        public static Task<double[,]> MultiplyAsync(this double[,] matrix1, double[,] matrix2)
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
        /// This method subtracts two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new vector with the result of subtraction between two vectors.</returns>
        public static Task<double[]> SubtractAsync(this double[] vector1, double[] vector2)
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
        /// This method sums three vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vectorsToAdd"></param>
        /// <returns></returns>
        public static Task<double[]> SumAsync(this double[] vector1, params double[][] vectors)
        {
            double[] vectorSum = vector1;

            for (int i = 0; i < vectorSum.Length; i++)
            {
                foreach(var vector in vectors)
                {
                    vectorSum[i] += vector[i];
                }
            }

            return Task.FromResult(vectorSum);
        }

        /// <summary>
        /// This method sums two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new array with the result of sum between two vectors.</returns>
        public static Task<double[]> SumAsync(this double[] vector1, double[] vector2)
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
        /// This method calculates the transposed matrix of a informed matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>The transposed matrix.</returns>
        public static Task<double[,]> TransposeMatrixAsync(this double[,] matrix)
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

        /// <summary>
        /// This method applies the boundary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="boundaryConditions"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public static Task<double[,]> ApplyBoundaryConditionsAsync(this double[,] matrix, bool[] boundaryConditions, uint numberOfTrueBoundaryConditions)
        {
            int count1, count2;

            double[,] matrixBC = new double[numberOfTrueBoundaryConditions, numberOfTrueBoundaryConditions];

            count1 = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                count2 = 0;

                if (boundaryConditions[i] == true)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (boundaryConditions[j] == true)
                        {
                            matrixBC[count1, count2] = matrix[i, j];

                            count2 += 1;
                        }
                    }

                    count1 += 1;
                }
            }

            return Task.FromResult(matrixBC);
        }

        /// <summary>
        /// This method applies the boundary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="boundaryConditions"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public static Task<double[]> ApplyBoundaryConditionsAsync(this double[] vector, bool[] boundaryConditions, uint numberOfTrueBoundaryConditions)
        {
            int count1 = 0;

            double[] matrixBC = new double[numberOfTrueBoundaryConditions];

            for (int i = 0; i < vector.Length; i++)
            {
                if (boundaryConditions[i] == true)
                {
                    matrixBC[count1] = vector[i];
                    count1 += 1;
                }
            }

            return Task.FromResult(matrixBC);
        }

        /// <summary>
        /// This method gets the vector's maximum value.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The vector's maximum value.</returns>
        public static double GetMaxValue(this double[] vector)
        {
            double maxValue = vector[0];
            for (int i = 1; i < vector.Length; i++)
            {
                if (vector[i] > maxValue)
                {
                    maxValue = vector[i];
                }
            }

            return maxValue;
        }

        /// <summary>
        /// This method gets the vector's minimum value.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The vector's minimum value.</returns>
        public static double GetMinValue(this double[] vector)
        {
            double minValue = vector[0];
            for (int i = 1; i < vector.Length; i++)
            {
                if (vector[i] < minValue)
                {
                    minValue = vector[i];
                }
            }

            return minValue;
        }

        /// <summary>
        /// This method multiplies each element in a vector by a value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>A new vector with the multiplication result.</returns>
        public static double[] MultiplyEachElement(this double[] vector, double value)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] * value;
            }

            return result;
        }

        /// <summary>
        /// This method divides each element in a vector by a each element in another vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="vectorToDivide"></param>
        /// <returns>A new vector with the division result.</returns>
        public static double[] Divide(this double[] vector, double[] vectorToDivide)
        {
            double[] result = new double[vector.Length];
            int i = 0;

            try
            {
                for (i = 0; i < vector.Length; i++)
                {
                    result[i] = vector[i] / vectorToDivide[i];
                }
            }
            catch (Exception ex)
            {
                throw new DivideByZeroException($"The vector to divide has invalid value: {vectorToDivide[i]} at position: {i}.", ex);
            }

            return result;
        }

        /// <summary>
        /// This method divides each element in a vector by a value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>A new vector with the division result.</returns>
        public static double[] DivideEachElement(this double[] vector, double value)
        {
            if (value == 0)
            {
                throw new DivideByZeroException($"The value cannot be zero on array extension method {nameof(DivideEachElement)}.");
            }

            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / value;
            }

            return result;
        }

        /// <summary>
        /// This method converts an array to a list.
        /// The matrix is ​​separated by columns to add to the list.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>A new instance of <see cref="List{double}"/> with the matrix's values.</returns>
        public static List<double[]> ConvertToListByColumns(this double[,] matrix)
        {
            List<double[]> result = new List<double[]>();

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var aux = new double[matrix.GetLength(1)];

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    aux[j] = matrix[j, i];
                }

                result.Add(aux);
            }

            return result;
        }

        /// <summary>
        /// This method calculates the norm of a vector.
        /// Equation:
        ///     norm = sqrt(vector[0]² + vector[1]² + ... + vector[n]²)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static double CalculateVectorNorm(this double[] vector)
        {
            double result = 0;

            for (int i = 0; i < vector.Length; i++)
            {
                result += Math.Pow(vector[i], 2);
            }

            return Math.Sqrt(result);
        }

        /// <summary>
        /// This method gets the maximum value below main diagonal.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>The maximum value below main diagonal.</returns>
        public static double GetMaxValueBelowMainDiagonal(this double[][] matrix)
        {
            // Get first value in the matrix below the main diagonal to initiate the comparison.
            double maxValue = matrix[1][0];

            // Begins in the second line.
            for (int i = 1; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    // Values below main diagonal.
                    if (i > j)
                    {
                        if (maxValue < matrix[i][j])
                        {
                            maxValue = matrix[i][j];
                        }
                    }
                }
            }

            return maxValue;
        }
    }

    /// <summary>
    /// It is responsible to create arrays.
    /// </summary>
    public class ArrayFactory
    {
        /// <summary>
        /// This method creates a vector with an unique value in the informed element positions with a size that is informed too.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="elementPositions"></param>
        /// <returns>A new instance of <see cref="double[]"/> with an unique value at the positions informed.</returns>
        public static Task<double[]> CreateVectorAsync(double value, uint size, uint[] elementPositions)
        {
            double[] newVector = new double[size];

            for (int i = 0; i < elementPositions.Length; i++)
            {
                newVector[elementPositions[i] - 1] = value;
            }

            return Task.FromResult(newVector);
        }

        /// <summary>
        /// This method creates a matrix with a unique value in all positions with a size that is informed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns>A new instance of <see cref="double[]"/> with an unique value at all positions.</returns>
        public static Task<double[]> CreateVectorAsync(double value, uint size)
        {
            double[] newVector = new double[size];

            for (int i = 0; i < size; i++)
            {
                newVector[i] = value;
            }

            return Task.FromResult(newVector);
        }
    }
}