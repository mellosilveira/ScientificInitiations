using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements
{
    /// <summary>
    /// It's responsible to calculate the beam vibration for finite element analysis.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam, TInput> : ICalculateVibration<TRequest, TRequestData, FiniteElementsResponse, FiniteElementsResponseData, TInput>
        where TProfile : Profile, new()
        where TRequestData : FiniteElementsRequestData<TProfile>
        where TRequest : FiniteElementsRequest<TProfile, TRequestData>
        where TBeam : IBeam<TProfile>, new()
        where TInput : NewmarkMethodInput, new()
    {
        /// <summary>
        /// Builds the beam.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TBeam> BuildBeam(TRequest request, uint degreesOfFreedom);
    }
}
