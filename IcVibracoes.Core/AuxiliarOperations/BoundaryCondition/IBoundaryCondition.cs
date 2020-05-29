using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.BoundaryCondition
{
    /// <summary>
    /// It's responsible to execute operations using the boundary conditions.
    /// </summary>
    public interface IBoundaryCondition
    {
        /// <summary>
        /// Applies the bondary conditions to a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<double[,]> ApplyBondaryConditions(double[,] matrix, bool[] bondaryConditions, uint size);

        /// <summary>
        /// Applies the bondary conditions to a vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="bondaryConditions"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<double[]> ApplyBondaryConditions(double[] vector, bool[] bondaryConditions, uint size);
    }
}