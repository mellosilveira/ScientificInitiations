namespace IcVibracoes.Core.AuxiliarOperations.File
{
    /// <summary>
    /// It's responsible to execute file operations.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        void Write(double time, double[] values, string path);

        /// <summary>
        /// Writes the values and its names in a file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        void Write(string name, double[] values, string path);
    }
}