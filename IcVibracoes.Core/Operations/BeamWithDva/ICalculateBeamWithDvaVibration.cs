using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithDynamicVibrationAbsorber;

namespace IcVibracoes.Core.Operations.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithDvaVibration<TProfile> : ICalculateVibration<CalculateBeamWithDvaVibrationRequest<TProfile>, BeamWithDvaRequestData<TProfile>, TProfile, BeamWithDva<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
