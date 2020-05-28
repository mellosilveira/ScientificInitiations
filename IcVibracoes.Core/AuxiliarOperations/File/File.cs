using System.IO;

namespace IcVibracoes.Core.AuxiliarOperations.File
{
    public class File : IFile
    {
        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// The linear displacement and angular displacement are separeted.
        /// In piezoelectric or DVA analysis, the additional values are separated too.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        public void Write(double time, double[] values, string path)
        {
            int length = values.Length;

            using (StreamWriter streamWriter = new StreamWriter(path, true))
            {
                streamWriter.Write(string.Format("{0}; ", time));

                for (int i = 0; i < length; i++)
                {
                    streamWriter.Write(string.Format("{0}; ", values[i]));
                }

                streamWriter.Write(streamWriter.NewLine);
            }
        }
    }
}
