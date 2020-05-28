using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements
{
    /// <summary>
    /// It represents the request content of Finite Elements operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    public abstract class FiniteElementsRequest<TProfile, TRequestData> : OperationRequestBase<TRequestData>
        where TProfile : Profile, new()
        where TRequestData : FiniteElementsRequestData<TProfile>
    {
    }
}
