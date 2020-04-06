using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithPiezoelectric;

namespace IcVibracoes.Core.Operations.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithPiezoelectricVibration<TProfile> : ICalculateVibration<CalculateBeamWithPiezoelectricVibrationRequest<TProfile>, PiezoelectricRequestData<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>
        where TProfile : Profile, new()
    {
    }
}