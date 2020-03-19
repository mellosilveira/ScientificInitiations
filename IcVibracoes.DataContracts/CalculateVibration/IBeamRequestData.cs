using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using System.Collections.Generic;

namespace IcVibracoes.DataContracts.CalculateVibration
{
    /// <summary>
    /// It represents the 'data' content of beam request operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamRequestData<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// Number of elements in the beam.
        /// </summary>
        uint NumberOfElements { get; set; }

        /// <summary>
        /// Beam material.
        /// </summary>
        string Material { get; set; }

        /// <summary>
        /// Beam first fastening.
        /// </summary>
        string FirstFastening { get; set; }

        /// <summary>
        /// Beam last fastening.
        /// </summary>
        string LastFastening { get; set; }

        /// <summary>
        /// Beam length.
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// Applied forces in the beam.
        /// </summary>
        List<Force> Forces { get; set; }

        /// <summary>
        /// Beam profile.
        /// </summary>
        TProfile Profile { get; set; }
    }
}