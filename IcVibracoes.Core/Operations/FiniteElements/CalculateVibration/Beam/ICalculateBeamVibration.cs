using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.FiniteElements.Beam;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamVibration<TProfile> : ICalculateVibration<BeamRequest<TProfile>, BeamRequestData<TProfile>, TProfile, Beam<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
