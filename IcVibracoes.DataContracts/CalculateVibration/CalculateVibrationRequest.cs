using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.CalculateVibration
{
    /// <summary>
    /// It represents the request content of CalculateBeam operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeamData"></typeparam>
    public abstract class CalculateVibrationRequest<TProfile, TBeamData> : OperationRequestBase
        where TProfile : Profile, new()
        where TBeamData : IBeamRequestData<TProfile>
    {
        public TBeamData BeamData { get; set; }

        public NewmarkMethodParameter MethodParameterData { get; set; }
    }
}
