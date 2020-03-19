using IcVibracoes.Common.Profiles;

namespace IcVibracoes.Core.Models.Beam
{
    /// <summary>
    /// It represents the analyzed beam.
    /// </summary>
    public class Beam<TProfile> : AbstractBeam<TProfile>
        where TProfile : Profile, new()
    {
    }
}
