using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the beam with DVA main matrixes.
    /// </summary>
    public interface IBeamWithDvaMainMatrix<TProfile> : IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Responsible to calculate the mass matrix of the beam.
        /// </summary>
        /// <param name="beamMass"></param>
        /// <param name="dvaMasses"></param>
        /// <param name="dvaNodePositions"></param>
        /// <returns></returns>
        Task<double[,]> CalculateMassWithDva(double[,] beamMass, double[] dvaMasses, uint[] dvaNodePositions);

        /// <summary>
        /// Responsible to calculate the stiffness matrix of the beam.
        /// </summary>
        /// <param name="beamStiffness"></param>
        /// <param name="dvaStiffness"></param>
        /// <param name="dvaNodePositions"></param>
        /// <returns></returns>
        Task<double[,]> CalculateStiffnessWithDva(double[,] beamStiffness, double[] dvaStiffness, uint[] dvaNodePositions);

        /// <summary>
        /// Responsible to calculate the bondary conditions matrix of the beam with dynamic vibration absorbers.
        /// </summary>
        /// <param name="firstFastening"></param>
        /// <param name="lastFastening"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <param name="numberOfDvas"></param>
        /// <returns></returns>
        Task<bool[]> CalculateBondaryCondition(Fastening firstFastening, Fastening lastFastening, uint degreesFreedomMaximum, uint numberOfDvas);
    }
}
