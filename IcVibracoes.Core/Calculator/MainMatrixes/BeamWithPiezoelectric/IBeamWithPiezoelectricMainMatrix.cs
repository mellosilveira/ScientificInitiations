using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the beam with piezoelectric main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamWithPiezoelectricMainMatrix<TProfile> : IMainMatrix<BeamWithPiezoelectric<TProfile>, TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// This method calculates the mass matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure mass matrix.</returns>
        double[,] CalculateStructureMass(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom);

        /// <summary>
        /// This method calculates stiffness matrix of an element of beam with piezoelectric plates.
        /// </summary>
        /// <param name="elasticityConstant"></param>
        /// <param name="momentOfInertia"></param>
        /// <param name="length"></param>
        /// <returns>The element's piezoelectric stiffness matrix.</returns>
        double[,] CalculatePiezoelectricElementStiffness(double elasticityConstant, double momentOfInertia, double length);

        /// <summary>
        /// This method calculates the stiffness matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure stiffness matrix.</returns>
        double[,] CalculateStructureStiffness(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom);

        /// <summary>
        /// This method calculates the electromechanical coupling matrix of an element of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The element's electromechanical coupling matrix.</returns>
        double[,] CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beam);

        /// <summary>
        /// This method calculates electromechanical coupling matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure piezoelectric electromechanical coupling matrix.</returns>
        double[,] CalculatePiezoelectricElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom);

        /// <summary>
        /// This method calculates the element piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="elementIndex"></param>
        /// <returns>The element's piezoelectric capacitance matrix.</returns>
        double[,] CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beam, uint elementIndex);

        /// <summary>
        /// This method calculates the piezoelectric capacitance matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The structure piezoelectric capacitance matrix.</returns>
        double[,] CalculatePiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beam);
    }
}
