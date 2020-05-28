using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements
{
    /// <summary>
    /// It's responsible to calculate the beam vibration at all contexts.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam> : IOperationBase<TRequest, TRequestData, FiniteElementsResponse, FiniteElementsResponseData>
        where TProfile : Profile, new()
        where TRequestData : FiniteElementsRequestData<TProfile>
        where TRequest : FiniteElementsRequest<TProfile, TRequestData>
        where TBeam : IBeam<TProfile>, new()
    { }
}
