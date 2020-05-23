using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements
{
    /// <summary>
    /// It represents the request content of Finite Elements operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    public abstract class FiniteElementsRequest<TProfile, TRequestData> : OperationRequestBase
        where TProfile : Profile, new()
        where TRequestData : FiniteElementsRequestData<TProfile>
    {
        /// <summary>
        /// It represents the 'data' content of Finite Elements request operation.
        /// </summary>
        public TRequestData BeamData { get; set; }
    }
}
