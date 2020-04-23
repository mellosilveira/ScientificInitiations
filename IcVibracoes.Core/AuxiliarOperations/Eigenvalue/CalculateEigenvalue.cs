using IcVibracoes.Common.ExtensionMethods;
using IcVibracoes.Core.Calculator.ArrayOperations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.Eigenvalue
{
    public class CalculateEigenvalue : ICalculateEigenvalue
    {
        private readonly IArrayOperation _arrayOperation;

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

            double[] error = new double[size];
            double[] lambda1 = new double[size];
            double[] lambda2 = new double[size];

            double[] y0 = new double[size];
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
                for(int i = 0; i < size; i++)
                {
                    error[i] = (lambda2[i] - lambda1[i]) / lambda2[i];
                }
            }
            while (error.GetMaxValue() > tolerance);

            int indexOfMininimumError = error.IndexOf(error.GetMinValue());
            double eigenvalue = lambda2.IndexOf(indexOfMininimumError);

            return eigenvalue;
        }
    }
}
