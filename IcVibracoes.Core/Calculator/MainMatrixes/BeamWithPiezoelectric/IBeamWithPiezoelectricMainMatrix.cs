using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models.Beams;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the beam with piezoelectric main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamWithPiezoelectricMainMatrix<TProfile> : IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// It's responsible to calculate piezoelectric mass matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateMass(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate piezoelectric stiffness matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateStiffness(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum);
        
        /// <summary>
        /// It's responsible to calculate piezoelectric element stiffness matrix.
        /// </summary>
        /// <param name="momentInertia"></param>
        /// <param name="elasticityConstant"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task<double[,]> CalculatePiezoelectricElementStiffness(double momentInertia, double elasticityConstant, double length);

        /// <summary>
        /// It's responsible to calculate piezoelectric electromechanical coupling matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculatePiezoelectricElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate piezoelectric element electromechanical coupling matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <returns></returns>
        Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric);

        /// <summary>
        /// It's responsible to calculate piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <returns></returns>
        Task<double[,]> CalculatePiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric);

        /// <summary>
        /// It's responsible to calculate element piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="elementIndex"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint elementIndex);

        /// <summary>
        /// It's responsible to calculate equivalent mass matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <param name="piezoelectricDegreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateEquivalentMass(double[,] mass, uint degreesFreedomMaximum, uint piezoelectricDegreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate equivalent stiffness matrix.
        /// </summary>
        /// <param name="stiffness"></param>
        /// <param name="piezoelectricElectromechanicalCoupling"></param>
        /// <param name="piezoelectricCapacitance"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <param name="piezoelectricDegreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateEquivalentStiffness(double[,] stiffness, double[,] piezoelectricElectromechanicalCoupling, double[,] piezoelectricCapacitance, uint degreesFreedomMaximum, uint piezoelectricDegreesFreedomMaximum);

        /// <summary>
        /// It's rewsponsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="firstFastening"></param>
        /// <param name="lastFastening"></param>
        /// <param name="numberOfNodes"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <returns></returns>
        Task<bool[]> CalculatePiezoelectricBondaryCondition(uint numberOfNodes, uint[] elementsWithPiezoelectric);
    }
}
