using System.IO;

namespace IcVibracoes.Core.ExtensionMethods
{
    /// <summary>
    /// It contains extension methods for class StreamWriter.
    /// </summary>
    public static class StreamWriterExtensions
    {
        /// <summary>
        /// This method writes the analysis result and the key to identify that values in a new line of file.
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public static void WriteResult(this StreamWriter streamWriter, double key, double[] result)
        {
            streamWriter.Write(string.Format("{0}; ", key));

            for (int i = 0; i < result.Length; i++)
            {
                streamWriter.Write(string.Format("{0}; ", result[i]));
            }

            streamWriter.Write(streamWriter.NewLine);
        }

        /// <summary>
        /// This method writes a matrix into a file.
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="matrix"></param>
        /// <param name="matrixName"></param>
        public static void WriteMatrix(this StreamWriter streamWriter, double[,] matrix, string matrixName)
        {
            streamWriter.Write(string.Format(matrixName));
            streamWriter.Write(streamWriter.NewLine);

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    streamWriter.Write(string.Format("{0}; ", matrix[i, j]));
                }

                streamWriter.Write(streamWriter.NewLine);
            }

            streamWriter.Write(streamWriter.NewLine);
        }

        /// <summary>
        /// This method writes a vector into a file.
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="vector"></param>
        /// <param name="matrixName"></param>
        public static void WriteVector(this StreamWriter streamWriter, double[] vector, string matrixName)
        {
            streamWriter.Write(string.Format(matrixName));
            streamWriter.Write(streamWriter.NewLine);

            for (int i = 0; i < vector.Length; i++)
            {
                streamWriter.Write(string.Format("{0}; ", vector[i]));
            }

            streamWriter.Write(streamWriter.NewLine);
        }
    }
}
