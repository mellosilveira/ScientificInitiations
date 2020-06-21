using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElement.Beam;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateBeamVibration<TProfile> : ICalculateVibration_FiniteElement<BeamRequest<TProfile>, TProfile, Beam<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
