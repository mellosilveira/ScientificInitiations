using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
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
        /// Responsible to calculate the beam mass matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateMass(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the beam element stiffness matrix.
        /// </summary>
        /// <param name="momentInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementStiffness(double momentInertia, double youngModulus, double elementLength);

        /// <summary>
        /// Responsible to calculate the beam stiffness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateStiffness(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<double[,]> CalculateDamping(double[,] mass, double[,] stiffness);

        /// <summary>
        /// It's rewsponsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="fastenings"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<bool[]> CalculateBondaryCondition(IDictionary<uint, FasteningType> fastenings, uint degreesFreedomMaximum);
    }
}