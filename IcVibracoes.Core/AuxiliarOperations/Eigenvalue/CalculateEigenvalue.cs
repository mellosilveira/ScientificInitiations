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
                double[] z1 = await this._arrayOperation.Multiply(matrix, y0, "Power Method");

                // Step 2 - Get the max value into vector z1.
                double alpha1 = z1.GetMaxValue();

                // Step 3 - Calculate vector y1.
                double[] y1 = z1.DivideEachElement(alpha1);

                // Step 4 - Calculate z2 using method equation.
                double[] z2 = await this._arrayOperation.Multiply(matrix, y1, "Power Method");

                //Step 5 - Calculate first value to eigenvalue (lambda1).
                lambda1 = z2.Divide(y1);

                // Step 6 - Get the max value into vector z2.
                double alpha2 = z2.GetMaxValue();

                // Step 7 - Calculate vector y2.
                double[] y2 = z2.DivideEachElement(alpha2);

                // Step 8 - Calculate vector z3.
                double[] z3 = await this._arrayOperation.Multiply(matrix, y2, "Power Method");

                // Step 9 - Calculate second value to eigenvalue (lambda2).
                lambda2 = z3.Divide(y2);

                // Step 10 - Calculate error.
                for (int i = 0; i < size; i++)
                {
                    error[i] = (lambda2[i] - lambda1[i]) / lambda2[i];
                }
            }
            while (error.GetMaxValue() > tolerance);

            int indexOfMininimumError = Array.IndexOf(error, error.GetMinValue());
            double eigenvalue = Array.IndexOf(lambda2, indexOfMininimumError);

            return eigenvalue;
        }

        /// <summary>
        /// Calculates the eigenvalues of a matrix using QR Decomposition.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public async Task<double[]> QR_Decomposition(double[,] matrix, double tolerance)
        {
            List<double[]> aVectors = matrix.ConvertColumnsToList();
            int size = aVectors.Count;

            var uVectors = new List<double[]>();
            var eVectors = new List<double[]>();

            for (int i = 0; i < size; i++)
            {
                double[] uVector = aVectors[i];

                for (int j = i - 1; j < 0; j--)
                {
                    double[] projection = await this.CalculateProjection(aVectors[i], uVectors[j]);
                    uVector = await this._arrayOperation.Subtract(uVector, projection);
                }

                double uNorm = uVector.CalculateVectorNorm();

                uVectors.Add(uVector);
                eVectors.Add(uVector.DivideEachElement(uNorm));
            }

            for (int i = 0; i < size; i++)
            {
                double[] aVector = new double[size];

                for (int j = 0; j < size; j++)
                {
                    double innerProduct = await this._arrayOperation.CalculateInnerProduct(eVectors[j], aVectors[i]);

                    aVector = await this._arrayOperation.Sum(eVectors[j].MultiplyEachElement(innerProduct), aVector);
                }

                aVectors[i] = aVector;
            }

            double[] eigenvalues = new double[size];
            for(int i = 0; i < size; i++)
            {
                eigenvalues[i] = await this._arrayOperation.CalculateInnerProduct(eVectors[i], aVectors[i]);
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
            double constant = await this._arrayOperation.CalculateInnerProduct(vector, baseVector) / await this._arrayOperation.CalculateInnerProduct(baseVector, baseVector);

            double[] result = baseVector.MultiplyEachElement(constant);

            return result;
        }
    }
}
