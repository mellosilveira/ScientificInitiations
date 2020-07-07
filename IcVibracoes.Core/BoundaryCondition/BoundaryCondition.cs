using System.Threading.Tasks;

namespace IcVibracoes.Core.BoundaryCondition
{
    /// <summary>
    /// It's responsible to execute operations using the boundary conditions.
    /// </summary>
    public class BoundaryCondition : IBoundaryCondition
    {
        /// <summary>
        /// Applies the bondary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Task<double[,]> Apply(double[,] matrix, bool[] bondaryConditions, uint size)
        {
            int count1, count2;

            double[,] matrixBC = new double[size, size];

            count1 = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                count2 = 0;

                if (bondaryConditions[i] == true)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (bondaryConditions[j] == true)
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
        /// Applies the bondary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Task<double[]> Apply(double[] vector, bool[] bondaryConditions, uint size)
        {
            int count1 = 0;

            double[] matrixBC = new double[size];

            for (int i = 0; i < vector.Length; i++)
            {
                if (bondaryConditions[i] == true)
                {
                    matrixBC[count1] = vector[i];
                    count1 += 1;
                }
            }

            return Task.FromResult(matrixBC);
        }
    }
}
