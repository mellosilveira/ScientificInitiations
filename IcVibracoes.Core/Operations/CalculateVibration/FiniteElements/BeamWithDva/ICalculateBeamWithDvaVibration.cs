using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithDvaVibration<TProfile, TInput> : ICalculateVibration_FiniteElements<BeamWithDvaRequest<TProfile>, BeamWithDvaRequestData<TProfile>, TProfile, BeamWithDva<TProfile>, TInput>
        where TProfile : Profile, new()
        where TInput : NewmarkMethodInput, new()
    {
    }
}
