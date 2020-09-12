using System;
using System.Collections.Generic;
using System.Linq;

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
        public static void AddPerNodePositionAsync(this double[,] matrixToAdd, double[] values, uint[] nodePositions)
        {
            int size = values.Length;

            for (int i = 0; i < size; i++)
            {
                matrixToAdd[2 * nodePositions[i], 2 * nodePositions[i]] += values[i];
            }
        }

        /// <summary>
        /// This method calculates the inner product between two matrixes.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>The inner product between two matrixes.</returns>
        public static double CalculateInnerProduct(this double[] vector1, double[] vector2)
        {
            double result = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }

            return result;
        }

        /// <summary>
        /// This method inverses a matrix using the Gauss-Jordan method.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>The inversed matrix using the Gauss-Jordan method.</returns>
        public static double[,] InverseMatrix(this double[,] matrix)
        {
            double[,] matrixCopy = matrix.Clone() as double[,];

            int n = matrixCopy.GetLength(0);
            double[,] matrizInv = new double[n, n];
            double pivot, p;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrizInv[i, j] = (i == j) ? 1 : 0;
                }
            }

            // Triangularization
            for (int i = 0; i < n; i++)
            {
                pivot = matrixCopy[i, i];
                if (pivot == 0)
                {
                    throw new DivideByZeroException($"Pivot cannot be zero at line {i}.");
                }

                for (int l = 0; l < n; l++)
                {
                    matrixCopy[i, l] = matrixCopy[i, l] / pivot;
                    matrizInv[i, l] = matrizInv[i, l] / pivot;
                }

                for (int k = i + 1; k < n; k++)
                {
                    p = matrixCopy[k, i];

                    for (int j = 0; j < n; j++)
                    {
                        matrixCopy[k, j] = matrixCopy[k, j] - p * matrixCopy[i, j];
                        matrizInv[k, j] = matrizInv[k, j] - p * matrizInv[i, j];
                    }
                }
            }

            // Retrosubstitution
            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = i - 1; k >= 0; k--)
                {
                    p = matrixCopy[k, i];

                    for (int j = n - 1; j >= 0; j--)
                    {
                        matrixCopy[k, j] = matrixCopy[k, j] - p * matrixCopy[i, j];
                        matrizInv[k, j] = matrizInv[k, j] - p * matrizInv[i, j];
                    }
                }
            }

            return matrizInv;
        }

        /// <summary>
        /// This method combines two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new array with the values combined of others two vectors.</returns>
        public static T[] CombineVectors<T>(this T[] vector1, T[] vector2)
        {
            int vector1Length = vector1.Length;
            int size = vector1Length + vector2.Length;
            T[] mergedVector = new T[size];

            for (int i = 0; i < size; i++)
            {
                mergedVector[i] = (i < vector1Length) ? vector1[i] : vector2[i - vector1Length];
            }

            return mergedVector;
        }

        /// <summary>
        /// This method multiplicates a matrix and a vector.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns>A new vector with the result of multiplication between a matrix and a vector.</returns>
        public static double[] Multiply(this double[,] matrix, double[] vector)
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

            return vectorMultiplication;
        }

        /// <summary>
        /// This method multiplicates two matrixes.
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns>A new matrix with the result of multiplication between two matrixes.</returns>
        public static double[,] Multiply(this double[,] matrix1, double[,] matrix2)
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

            return result;
        }

        /// <summary>
        /// This method subtracts two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new vector with the result of subtraction between two vectors.</returns>
        public static double[] SubtractAsync(this double[] vector1, double[] vector2)
        {
            return vector1.Zip(vector2, (v1, v2) => v1 - v2).ToArray();
        }

        /// <summary>
        /// This method sums any vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static double[] Sum(this double[] vector1, params double[][] vectors)
        {
            double[] vectorSum = vector1;

            for (int i = 0; i < vectorSum.Length; i++)
            {
                foreach (var vector in vectors)
                {
                    vectorSum[i] += vector[i];
                }
            }

            return vectorSum;
        }

        /// <summary>
        /// This method sums two vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns>A new array with the result of sum between two vectors.</returns>
        public static double[] Sum(this double[] vector1, double[] vector2)
        {
            return vector1.Zip(vector2, (v1, v2) => v1 + v2).ToArray();
        }

        /// <summary>
        /// This method calculates the transposed matrix of a informed matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>The transposed matrix.</returns>
        public static double[,] TransposeMatrixAsync(this double[,] matrix)
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

            return matrixTransposed;
        }

        /// <summary>
        /// This method applies the boundary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="boundaryConditions"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public static double[,] ApplyBoundaryConditions(this double[,] matrix, bool[] boundaryConditions, uint numberOfTrueBoundaryConditions)
        {
            double[,] matrixBc = new double[numberOfTrueBoundaryConditions, numberOfTrueBoundaryConditions];

            var count1 = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var count2 = 0;

                if (boundaryConditions[i] == true)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (boundaryConditions[j] == true)
                        {
                            matrixBc[count1, count2] = matrix[i, j];

                            count2 += 1;
                        }
                    }

                    count1 += 1;
                }
            }

            return matrixBc;
        }

        /// <summary>
        /// This method applies the boundary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="boundaryConditions"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public static double[] ApplyBoundaryConditions(this double[] vector, bool[] boundaryConditions, uint numberOfTrueBoundaryConditions)
        {
            return vector
                .Where((item, index) => boundaryConditions[index])
                .ToArray();
        }
        
        /// <summary>
        /// This method gets the vector's minimum value.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The vector's minimum value.</returns>
        public static double GetMinValue(this double[] vector)
        {
            return vector.Min();
        }

        /// <summary>
        /// This method multiplies each element in a vector by a value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>A new vector with the multiplication result.</returns>
        public static double[] MultiplyEachElement(this double[] vector, double value)
        {
            return vector
                .Select(item => item * value)
                .ToArray();
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
            double result = vector.Sum(v => Math.Pow(v, 2));

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
}