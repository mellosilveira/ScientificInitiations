using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithPiezoelectricVibration<TProfile, TInput> : ICalculateVibration_FiniteElements<BeamWithPiezoelectricRequest<TProfile>, PiezoelectricRequestData<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>, TInput>
        where TProfile : Profile, new()
        where TInput : NewmarkMethodInput, new()
    {
    }
}