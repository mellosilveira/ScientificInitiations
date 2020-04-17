using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.BeamRequestData
{
    /// <summary>
    /// It's responsible to validate the beam request.
    /// </summary>
    /// <typeparam name="TBeamData"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamRequestDataValidator<TBeamData, TProfile>
        where TProfile : Profile, new()
        where TBeamData : FiniteElementsRequestData<TProfile>
    {
        /// <summary>
        /// Validates the beam request data.
        /// </summary>
        /// <param name="beamData"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<bool> ValidateBeamData(TBeamData beamData, OperationResponseBase response);
    }
}
