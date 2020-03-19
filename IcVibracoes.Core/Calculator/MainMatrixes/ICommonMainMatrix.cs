using IcVibracoes.Models.Beam.Characteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Calculator.MainMatrixes
{
    /// <summary>
    /// It's responsible to build the main matrixes in the analysis.
    /// </summary>
    public interface ICommonMainMatrix
    {
        /// <summary>
        /// It's responsible to calculate the element mass matrix.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="specificMass"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementMass(double area, double specificMass, double elementLength);

        /// <summary>
        /// It's responsible to calculate the damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="hardness"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<double[,]> CalculateDamping(double[,] mass, double[,] hardness, uint size);
        
        /// <summary>
        /// It's rewsponsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="firstFastening"></param>
        /// <param name="lastFastening"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<bool[]> CalculateBondaryCondition(Fastening firstFastening, Fastening lastFastening, uint degreesFreedomMaximum);
    }
}
