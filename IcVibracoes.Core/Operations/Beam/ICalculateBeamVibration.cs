using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Models.Beam;
using IcVibracoes.DataContracts.CalculateVibration.Beam;

namespace IcVibracoes.Core.Operations.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamVibration<TProfile> : ICalculateVibration<CalculateBeamVibrationRequest<TProfile>, BeamRequestData<TProfile>, TProfile, Beam<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
