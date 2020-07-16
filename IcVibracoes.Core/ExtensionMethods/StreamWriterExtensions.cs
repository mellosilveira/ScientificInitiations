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
        /// <param name="vector"></param>
        public static void WriteResult(this StreamWriter streamWriter, double key, double[] result)
        {
            streamWriter.Write(string.Format("{0}; ", key));

            for (int i = 0; i < result.Length; i++)
            {
                streamWriter.Write(string.Format("{0}; ", result[i]));
            }

            streamWriter.Write(streamWriter.NewLine);
        }
    }
}
