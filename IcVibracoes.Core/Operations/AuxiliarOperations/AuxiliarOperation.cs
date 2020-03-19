using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Models;
using System;
using System.IO;

namespace IcVibracoes.Methods.AuxiliarOperations
{
    public class AuxiliarOperation : IAuxiliarOperation
    {
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

        public double[] ApplyBondaryConditions(double[] matrix, bool[] bondaryConditions, uint size)
        {
            int i, count1 = 0;

            int n = matrix.GetLength(0);

            double[] matrixCC = new double[size];

            for (i = 0; i < n; i++)
            {
                if (bondaryConditions[i] == true)
                {
                    matrixCC[count1] = matrix[i];
                    count1 += 1;
                }
            }

            return matrixCC;
        }

        public uint CalculateDegreesFreedomMaximum(uint numberOfElements)
        {
            return (numberOfElements + 1) * Constants.NodesPerElement;
        }

        public void WriteInFile(double time, double[] values)
        {
            const string path = "C:/Users/bruno/OneDrive/Documentos/GitHub/IC_Vibra-es/IcVibrations/Solutions/TestSolution.csv";

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

        public void WriteInFile(double angularFrequency)
        {
            const string path = "C:/Users/bruno/OneDrive/Documentos/GitHub/IC_Vibra-es/IcVibrations/Solutions/TestSolution.csv";

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