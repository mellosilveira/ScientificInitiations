using IcVibracoes.Core.Models;
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

            for (int i = 0; i < size; i++)
            {
                count2 = 0;

                if (bondaryConditions[i] == true)
                {
                    for (int j = 0; j < size; j++)
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

            for (int i = 0; i < size; i++)
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
            return (numberOfElements + 1) * Constant.NodesPerElement;
        }

        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        public void WriteInFile(double time, double[] values, string path)
        {
            using (StreamWriter streamWriter = new StreamWriter(path, true))
            {
                streamWriter.Write(string.Format("{0}; ", time));

                for (int i = 0; i < values.Length; i++)
                {
                    streamWriter.Write(string.Format("{0}; ", values[i]));
                }

                streamWriter.Write(streamWriter.NewLine);
            }
        }

        /// <summary>
        /// Writes the angular frequency in a file to start calculating the solution.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="path"></param>
        public void WriteInFile(double angularFrequency, string path)
        {
            using (StreamWriter streamWriter = new StreamWriter(path, true))
            {
                streamWriter.WriteLine($"Angular frequency: {angularFrequency}");
            }
        }

        /// <summary>
        /// Create a path to the files with the analysis solution.
        /// </summary>
        /// <param name="analysisType"></param>
        /// <param name="initialAngularFrequency"></param>
        /// <param name="finalAngularFrequency"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public string CreateSolutionPath(string analysisType, double initialAngularFrequency, double? finalAngularFrequency, uint numberOfElements)
        {
            string path = Directory.GetCurrentDirectory();

            string folderName = Path.GetFileName(path);

            string previousPath = path.Substring(0, path.Length - folderName.Length);

            if (finalAngularFrequency == null)
            {
                path = Path.Combine(previousPath, "Solutions", $"{analysisType}_w0={Math.Round(initialAngularFrequency, 2)}_nEl={numberOfElements}.csv");
            }
            else
            {
                path = Path.Combine(previousPath, "Solutions", $"{analysisType}_w0={Math.Round(initialAngularFrequency, 2)}_wf={finalAngularFrequency}_nEl={numberOfElements}.csv");
            }

            if (File.Exists(path))
            {
                throw new IOException($"File already exist. File name: {Path.GetFileName(path)}.");
            }

            return path;
        }
    }
}