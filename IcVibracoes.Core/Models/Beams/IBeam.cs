using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.BeamCharacteristics;

namespace IcVibracoes.Core.Models.Beams
{
    /// <summary>
    /// It represents the beam abstract object.
    /// </summary>
    public interface IBeam<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Number of elements.
        /// </summary>
        public uint NumberOfElements { get; set; }

        /// <summary>
        /// Material. Can be: Steel 1020, Steel 4130, Aluminium.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// First fastening. Can be: fixed, pinned and simple.
        /// </summary>
        public Fastening FirstFastening { get; set; }

        /// <summary>
        /// Last fastening. Can be: fixed, pinned and simple.
        /// </summary>
        public Fastening LastFastening { get; set; }

        /// <summary>
        /// Length.
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Force matrix. Matrix size: degrees freedom maximum.
        /// </summary>
        public double[] Forces { get; set; }

        /// <summary>
        /// Geometric property.
        /// </summary>
        public GeometricProperty GeometricProperty { get; set; }

        /// <summary>
        /// Beam profile.
        /// </summary>
        public TProfile Profile { get; set; }
    }
}
