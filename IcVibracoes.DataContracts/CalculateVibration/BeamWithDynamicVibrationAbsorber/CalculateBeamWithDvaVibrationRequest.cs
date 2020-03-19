using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.CalculateVibration.BeamWithDynamicVibrationAbsorber
{
    /// <summary>
    /// It represents the request content of CalculateBeamWithDva operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class CalculateBeamWithDvaVibrationRequest<TProfile> : CalculateVibrationRequest<TProfile, BeamWithDvaRequestData<TProfile>>
        where TProfile : Profile, new()
    {
    }
}
