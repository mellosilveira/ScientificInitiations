using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the beam with DVA main matrixes.
    /// </summary>
    public interface IBeamWithDvaMainMatrix<TProfile> : IMainMatrix<BeamWithDva<TProfile>, TProfile>
        where TProfile : Profile, new()
    { }
}
