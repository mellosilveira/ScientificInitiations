using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.CalculateVibration.Beam
{
    /// <summary>
    /// It represents the 'data' content of beam request operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamRequestData<TProfile> : CalculateVibrationRequestData<TProfile>
        where TProfile : Profile
    {
    }
}