using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.DataContracts.FiniteElement;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement
{
    /// <summary>
    /// It's responsible to calculate the beam vibration using finite element concepts.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface ICalculateVibration_FiniteElement<TRequest, TProfile, TBeam> : ICalculateVibration<TRequest, FiniteElementResponse, FiniteElementResponseData, FiniteElementMethodInput>
        where TRequest : FiniteElementRequest<TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        /// <summary>
        /// This method creates a new instance of class <see cref="TBeam"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="TBeam"/>.</returns>
        Task<TBeam> BuildBeam(TRequest request, uint degreesOfFreedom, FiniteElementResponse response);
    }
}
