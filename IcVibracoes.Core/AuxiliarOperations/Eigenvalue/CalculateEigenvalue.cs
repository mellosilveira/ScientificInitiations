using IcVibracoes.Common.ExtensionMethods;
using IcVibracoes.Core.Calculator.ArrayOperations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.Eigenvalue
{
    /// <summary>
    /// It's responsible to calculate the eigenvalues to a matrix.
    /// It's used in natural frequency calculation.
    /// </summary>
    public class CalculateEigenvalue : ICalculateEigenvalue
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public CalculateEigenvalue(
            IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

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
        public async Task<double> PowerMethod(double[,] matrix, double tolerance)
        {
            int size = matrix.GetLength(0);

            var error = new double[size];
            var lambda1 = new double[size];
            var lambda2 = new double[size];

            var y0 = new double[size];
            for (int i = 0; i < size; i++)
            {
                y0[i] = 1;
            }

            do
            {
                // Step 1  - Calculate z1 using method equation. 
                double[] z1 = await this._arrayOperation.Multiply(matrix, y0, "Power Method").ConfigureAwait(false);

                // Step 2 - Get the max value into vector z1.
                double alpha1 = z1.GetMaxValue();

                // Step 3 - Calculate vector y1.
                double[] y1 = z1.DivideEachElement(alpha1);

                // Step 4 - Calculate z2 using method equation.
                double[] z2 = await this._arrayOperation.Multiply(matrix, y1, "Power Method").ConfigureAwait(false);

                //Step 5 - Calculate first value to eigenvalue (lambda1).
                lambda1 = z2.Divide(y1);

                // Step 6 - Get the max value into vector z2.
                double alpha2 = z2.GetMaxValue();

                // Step 7 - Calculate vector y2.
                double[] y2 = z2.DivideEachElement(alpha2);

                // Step 8 - Calculate vector z3.
                double[] z3 = await this._arrayOperation.Multiply(matrix, y2, "Power Method").ConfigureAwait(false);

                // Step 9 - Calculate second value to eigenvalue (lambda2).
                lambda2 = z3.Divide(y2);

                // Step 10 - Calculate error.
                for (int i = 0; i < size; i++)
                {
                    error[i] = (lambda2[i] - lambda1[i]) / lambda2[i];
                }
            }
            while (error.GetMaxValue() > tolerance);

            // Step 11 - Get the eigenvalue with smallest error.
            int indexOfSmallestError = Array.IndexOf(error, error.GetMinValue());
            double eigenvalue = Array.IndexOf(lambda2, indexOfSmallestError);

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
        public async Task<double[]> QR_Decomposition(double[,] matrix, double tolerance)
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
                        double[] projection = await this.CalculateProjection(matrixA[i], vectorsU[j]).ConfigureAwait(false);
                        vectorU = await this._arrayOperation.Subtract(vectorU, projection).ConfigureAwait(false);
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
                        double innerProduct = await this._arrayOperation.CalculateInnerProduct(vectorsE[j], matrixA[i]).ConfigureAwait(false);

                        aVector = await this._arrayOperation.Sum(vectorsE[j].MultiplyEachElement(innerProduct), aVector).ConfigureAwait(false);
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
                        if (i <= j)
                        {
                            matrixQ[i, j] = await this._arrayOperation.CalculateInnerProduct(vectorsE[i], matrixA[j]).ConfigureAwait(false);
                        }
                        else
                        {
                            matrixR[i, j] = 0;
                        }
                    }
                }

                double[,] transposedMatrixQ = await this._arrayOperation.TransposeMatrix(matrixQ).ConfigureAwait(false);

                matrixA = (await this._arrayOperation.Multiply(matrixR, transposedMatrixQ).ConfigureAwait(false)).ConvertToListByColumns();
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
        private async Task<double[]> CalculateProjection(double[] vector, double[] baseVector)
        {
            double constant = await this._arrayOperation.CalculateInnerProduct(vector, baseVector).ConfigureAwait(false) / await this._arrayOperation.CalculateInnerProduct(baseVector, baseVector).ConfigureAwait(false);

            double[] result = baseVector.MultiplyEachElement(constant);

            return result;
        }
    }
}
