using IcVibracoes.Core.Models;
using System;
using System.IO;

namespace IcVibracoes.Methods.AuxiliarOperations
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
            int i, j, count1, count2;

            int n = matrix.GetLength(0);

            double[,] matrixBC = new double[size, size];

            count1 = 0;

            for (i = 0; i < n; i++)
            {
                count2 = 0;

                if (bondaryConditions[i] == true)
                {
                    for (j = 0; j < n; j++)
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
            int i, count1 = 0;

            int n = vector.GetLength(0);

            double[] matrixCC = new double[size];

            for (i = 0; i < n; i++)
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
        /// Calculates the degrees freedom maximum.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public uint CalculateDegreesFreedomMaximum(uint numberOfElements)
        {
            return (numberOfElements + 1) * Constants.NodesPerElement;
        }

        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        public void WriteInFile(double time, double[] values, string path)
        {
            StreamWriter streamWriter = new StreamWriter(path, true);

            try
            {
                using (StreamWriter sw = streamWriter)
                {
                    sw.Write(sw.NewLine);

                    sw.Write(string.Format("{0}; ", time));

                    for (int i = 0; i < values.Length / 2; i++)
                    {
                        sw.Write(string.Format("{0}; ", values[2 * i]));
                    }

                    sw.Write(" ;");

                    for (int i = 0; i < values.Length / 2; i++)
                    {
                        sw.Write(string.Format("{0}; ", values[2 * i + 1]));
                    }
                }
            }
            catch
            {
                throw new Exception("Couldn't open file.");
            }
        }

        /// <summary>
        /// Writes the angular frequency in a file to start calculating the solution.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="path"></param>
        public void WriteInFile(double angularFrequency, string path)
        {
            StreamWriter streamWriter = new StreamWriter(path, true);

            try
            {
                using (StreamWriter sw = streamWriter)
                {
                    sw.WriteLine($"Angular frequency: {angularFrequency}");
                }
            }
            catch
            {
                throw new Exception("Couldn't open file.");
            }
        }
    }
}