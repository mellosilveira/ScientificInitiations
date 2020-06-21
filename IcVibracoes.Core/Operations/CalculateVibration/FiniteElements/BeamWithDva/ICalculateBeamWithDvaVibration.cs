using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElement.BeamWithDynamicVibrationAbsorber;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithDvaVibration<TProfile> : ICalculateVibration_FiniteElement<BeamWithDvaRequest<TProfile>, TProfile, BeamWithDva<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
