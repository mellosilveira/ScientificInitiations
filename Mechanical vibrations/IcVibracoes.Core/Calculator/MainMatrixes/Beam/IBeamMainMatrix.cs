using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamMainMatrix<TProfile> : IMainMatrix<Beam<TProfile>, TProfile>
        where TProfile : Profile, new()
    { }
}