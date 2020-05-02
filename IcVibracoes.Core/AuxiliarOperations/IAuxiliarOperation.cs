namespace IcVibracoes.Core.AuxiliarOperations
{
    /// <summary>
    /// It contains auxiliar operations to the solve specific problems in the project.
    /// </summary>
    public interface IAuxiliarOperation
    {
        /// <summary>
        /// Applies the bondary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        double[,] ApplyBondaryConditions(double[,] matrix, bool[] bondaryConditions, uint size);

        /// <summary>
        /// Applies the bondary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        double[] ApplyBondaryConditions(double[] vector, bool[] bondaryConditions, uint size);

        /// <summary>
        /// Writes the values ​​corresponding to an instant of time in a file.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        void WriteInFile(double time, double[] values, string path);

        /// <summary>
        /// Writes the angular frequency in a file to start calculating the solution.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="path"></param>
        void WriteInFile(double angularFrequency, string path);

        /// <summary>
        /// Create a path to the files with the analysis solution.
        /// </summary>
        /// <returns></returns>
        string CreateSolutionPath(string analysisType, double initialAngularFrequency, double? finalAngularFrequency, uint numberOfElements);
    }
}