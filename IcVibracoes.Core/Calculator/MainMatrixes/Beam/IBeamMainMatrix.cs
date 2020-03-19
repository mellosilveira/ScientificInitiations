using IcVibracoes.Calculator.MainMatrixes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beam;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamMainMatrix<TProfile> : ICommonMainMatrix
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Responsible to calculate the beam mass matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateMass(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// Responsible to calculate the beam hardness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateHardness(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the beam element hardness matrix.
        /// </summary>
        /// <param name="momentInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementHardness(double momentInertia, double youngModulus, double elementLength);
    }
}