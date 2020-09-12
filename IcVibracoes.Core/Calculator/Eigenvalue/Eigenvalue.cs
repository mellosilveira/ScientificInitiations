using IcVibracoes.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IcVibracoes.Core.Calculator.Eigenvalue
{
    /// <summary>
    /// It's responsible to calculate the eigenvalues to a matrix.
    /// It's used in natural frequency calculation.
    /// </summary>
    public class Eigenvalue : IEigenvalue
    {
        /// <summary>
        /// Calculates the biggest eigenvalue using Power Method.
        /// Equations to be used:
        /// [zk+1] = [Matrix] x [yk].
        /// alphak = max([zk])
        /// [yk] = [zk] / alphak
        /// [eigenvalue] = [zk+1] / [yk].
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public double PowerMethod(double[,] matrix, double tolerance)
        {
            int size = matrix.GetLength(0);

            var error = new double[size];
            double[] lambda2;

            var y0 = new double[size];
            Array.Fill(y0, 1);

            do
            {
                // Step 1  - Calculate z1 using method equation. 
                double[] z1 = matrix.Multiply(y0);

                // Step 2 - Get the max value into vector z1.
                double alpha1 = z1.Max();

                // Step 3 - Calculate vector y1.
                double[] y1 = z1.DivideEachElement(alpha1);

                // Step 4 - Calculate z2 using method equation.
                double[] z2 = matrix.Multiply(y1);

                //Step 5 - Calculate first value to eigenvalue (lambda1).
                var lambda1 = z2.Divide(y1);

                // Step 6 - Get the max value into vector z2.
                double alpha2 = z2.Max();

                // Step 7 - Calculate vector y2.
                double[] y2 = z2.DivideEachElement(alpha2);

                // Step 8 - Calculate vector z3.
                double[] z3 = matrix.Multiply(y2);

                // Step 9 - Calculate second value to eigenvalue (lambda2).
                lambda2 = z3.Divide(y2);

                // Step 10 - Calculate error.
                for (int i = 0; i < size; i++)
                {
                    error[i] = (lambda2[i] - lambda1[i]) / lambda2[i];
                }
            }
            while (error.Max() > tolerance);

            // Step 11 - Get the eigenvalue with smallest error.
            int indexOfSmallestError = Array.IndexOf(error, error.Min());
            double eigenvalue = lambda2[indexOfSmallestError];

            return eigenvalue;
        }

        /// <summary>
        /// Calculates the eigenvalues of a matrix using QR Decomposition.
        /// Logic used:
        ///     https://sites.icmc.usp.br/andretta/ensino/aulas/sme0301-1-10/AutovaloresFrancis.pdf
        ///     https://pt.wikipedia.org/wiki/Decomposi%C3%A7%C3%A3o_QR
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public double[] QR_Decomposition(double[,] matrix, double tolerance)
        {
            List<double[]> matrixA = matrix.ConvertToListByColumns();
            int size = matrixA.Count;

            var vectorsU = new List<double[]>();
            var vectorsE = new List<double[]>();

            do
            {
                for (int i = 0; i < size; i++)
                {
                    double[] vectorU = matrixA[i];

                    for (int j = i - 1; j < 0; j--)
                    {
                        double[] projection = CalculateProjection(matrixA[i], vectorsU[j]);
                        vectorU = vectorU.SubtractAsync(projection);
                    }

                    double uNorm = vectorU.CalculateVectorNorm();

                    vectorsU.Add(vectorU);
                    vectorsE.Add(vectorU.DivideEachElement(uNorm));
                }

                for (int i = 0; i < size; i++)
                {
                    double[] aVector = new double[size];

                    for (int j = 0; j < size; j++)
                    {
                        double innerProduct = vectorsE[j].CalculateInnerProduct(matrixA[i]);

                        aVector = (vectorsE[j].MultiplyEachElement(innerProduct)).Sum(aVector);
                    }

                    matrixA[i] = aVector;
                }

                double[,] matrixQ = new double[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrixQ[i, j] = vectorsE[i][j];
                    }
                }

                double[,] matrixR = new double[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrixR[i, j] = 0;
                        if (i <= j)
                        {
                            matrixQ[i, j] = vectorsE[i].CalculateInnerProduct(matrixA[j]);
                        }
                    }
                }

                double[,] transposedMatrixQ = matrixQ.TransposeMatrixAsync();

                matrixA = (matrixR.Multiply(transposedMatrixQ)).ConvertToListByColumns();
            }
            while (matrixA.ToArray().GetMaxValueBelowMainDiagonal() > tolerance);

            double[] eigenvalues = new double[size];
            for (int i = 0; i < size; i++)
            {
                eigenvalues[i] = matrixA[i][i];
            }

            return eigenvalues;
        }

        /// <summary>
        /// Calculates the projection of vector in a based that must be passed.
        /// Equation used:
        ///     proj(a) = (<u,a>/<u,u>) * u
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        /// <returns></returns>
        private static double[] CalculateProjection(double[] vector, double[] baseVector)
        {
            double numerator = vector.CalculateInnerProduct(baseVector);
            double denominator = baseVector.CalculateInnerProduct(baseVector);

            double constant = numerator / denominator;
            double[] result = baseVector.MultiplyEachElement(constant);

            return result;
        }
    }
}
