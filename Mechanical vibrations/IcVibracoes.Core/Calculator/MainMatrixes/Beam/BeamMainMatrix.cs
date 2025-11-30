using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamMainMatrix<TProfile> : MainMatrix<Beam<TProfile>, TProfile>, IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    { }
}