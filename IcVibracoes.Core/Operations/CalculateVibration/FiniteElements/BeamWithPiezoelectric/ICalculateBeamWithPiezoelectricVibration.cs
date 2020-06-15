using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamWithPiezoelectricVibration<TProfile> : ICalculateVibration_FiniteElements<BeamWithPiezoelectricRequest<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>
        where TProfile : Profile, new()
    {
    }
}