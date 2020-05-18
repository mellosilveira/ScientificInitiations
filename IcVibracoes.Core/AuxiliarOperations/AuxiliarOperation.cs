using System;
using System.IO;

namespace IcVibracoes.Core.AuxiliarOperations
{
    /// <summary>
    /// It contains auxiliar operations to the solve specific problems in the project.
    /// </summary>
    public class AuxiliarOperation : IAuxiliarOperation
    {
        /// <summary>
        /// Applies the bondary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public double[,] ApplyBondaryConditions(double[,] matrix, bool[] bondaryConditions, uint size)
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

            return matrixBC;
        }

        /// <summary>
        /// Applies the bondary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public double[] ApplyBondaryConditions(double[] vector, bool[] bondaryConditions, uint size)
        {
            int count1 = 0;

            double[] matrixCC = new double[size];

            for (int i = 0; i < vector.Length; i++)
            {
                if (bondaryConditions[i] == true)
                {
                    matrixCC[count1] = vector[i];
                    count1 += 1;
                }
            }

            return matrixCC;
        }

        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// The linear displacement and angular displacement are separeted.
        /// In piezoelectric or DVA analysis, the additional values are separated too.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        /// <param name="degreesOfFreedom"></param>
        public void WriteInFile(double time, double[] values, string path, uint degreesOfFreedom)
        {
            int length = values.Length;

            if (length == degreesOfFreedom)
            {
                using (StreamWriter streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.Write(string.Format("{0}; ", time));

                    for (int i = 0; i < length / 2; i++)
                    {
                        streamWriter.Write(string.Format("{0}; ", values[2 * i]));
                    }

                    for (int i = 0; i < length / 2; i++)
                    {
                        streamWriter.Write(string.Format("{0}; ", values[2 * i + 1]));
                    }

                    streamWriter.Write(streamWriter.NewLine);
                }
            }
            else if (length > degreesOfFreedom)
            {
                using (StreamWriter streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.Write(string.Format("{0}; ", time));

                    for (int i = 0; i < degreesOfFreedom / 2; i++)
                    {
                        streamWriter.Write(string.Format("{0}; ", values[2 * i]));
                    }

                    for (int i = 0; i < degreesOfFreedom / 2; i++)
                    {
                        streamWriter.Write(string.Format("{0}; ", values[2 * i + 1]));
                    }

                    for (uint i = degreesOfFreedom; i < length; i++)
                    {
                        streamWriter.Write(string.Format("{0}; ", values[i]));
                    }

                    streamWriter.Write(streamWriter.NewLine);
                }
            }
        }
    }
}