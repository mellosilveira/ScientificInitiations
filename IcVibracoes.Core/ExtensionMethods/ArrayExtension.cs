using System;
using System.Collections.Generic;

namespace IcVibracoes.Core.ExtensionMethods
{
    public static class VectorExtension
    {
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

        public static double[] MultiplyEachElement(this double[] vector, double value)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] * value;
            }

            return result;
        }

        public static double[] Divide(this double[] vector, double[] vectorToDivide)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / vectorToDivide[i];
            }

            return result;
        }

        public static double[] DivideEachElement(this double[] vector, double value)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / value;
            }

            return result;
        }

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

                result.AddRange(new List<double[]> { aux });
            }

            return result;
        }

        /// <summary>
        /// Calculates the norm of a vector.
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
