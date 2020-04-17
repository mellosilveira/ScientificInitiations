using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements
{
    /// <summary>
    /// It represents the request content of Finite Elements operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeamData"></typeparam>
    public abstract class FiniteElementsRequest<TProfile, TBeamData> : OperationRequestBase
        where TProfile : Profile, new()
        where TBeamData : FiniteElementsRequestData<TProfile>
    {
        public TBeamData BeamData { get; set; }

        public NewmarkMethodParameter MethodParameterData { get; set; }
    }
}
