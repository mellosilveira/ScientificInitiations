using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes
{
    /// <summary>
    /// It's responsible to calculate the structure main matrixes to Finite Element Analysis.
    /// </summary>
    /// <typeparam name="TBeam"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public interface IMainMatrix<TBeam, TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        /// <summary>
        /// This method calculates the element's mass matrix.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="specificMass"></param>
        /// <param name="elementLength"></param>
        /// <returns>The elementary mass matrix.</returns>
        Task<double[,]> CalculateElementMass(double area, double specificMass, double elementLength);

        /// <summary>
        /// This method calculates the beam's mass matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure mass matrix.</returns>
        Task<double[,]> CalculateMass(TBeam beam, uint degreesOfFreedom);

        /// <summary>
        /// This method calculates the element's stiffness matrix.
        /// </summary>
        /// <param name="momentOfInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns>The elementary stiffness matrix.</returns>
        Task<double[,]> CalculateElementStiffness(double momentOfInertia, double youngModulus, double elementLength);

        /// <summary>
        /// This method calculates the beam's stiffness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure stiffness matrix.</returns>
        Task<double[,]> CalculateStiffness(TBeam beam, uint degreesOfFreedom);

        /// <summary>
        /// This method calculates the beam's damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns>The structure damping matrix.</returns>
        Task<double[,]> CalculateDamping(double[,] mass, double[,] stiffness);

        /// <summary>
        /// This method calculates the beam's force matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The structure force matrix.</returns>
        Task<double[]> CalculateForce(TBeam beam);

        /// <summary>
        /// This method builds the boundary condition matrix and the number of true boundary conditions.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The boundary conditions matrix and the number of true boundary conditions.</returns>
        Task<(bool[], uint)> CalculateBoundaryConditions(TBeam beam, uint degreesOfFreedom);
    }
}
