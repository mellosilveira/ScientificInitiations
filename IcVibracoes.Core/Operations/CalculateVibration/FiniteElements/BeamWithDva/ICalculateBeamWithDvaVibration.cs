using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithDvaVibration<TProfile> : ICalculateVibration_FiniteElements<BeamWithDvaRequest<TProfile>, BeamWithDvaRequestData<TProfile>, TProfile, BeamWithDva<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
