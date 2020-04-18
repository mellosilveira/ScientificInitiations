using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.BeamRequestData
{
    /// <summary>
    /// It's responsible to validate the beam request.
    /// </summary>
    /// <typeparam name="TBeamData"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamRequestDataValidator<TBeamData, TProfile> : IBeamRequestDataValidator<TBeamData, TProfile>
        where TProfile : Profile, new()
        where TBeamData : FiniteElementsRequestData<TProfile>
    {
        /// <summary>
        /// Validates the beam request data.
        /// </summary>
        /// <param name="beamData"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Task<bool> ValidateBeamData(TBeamData beamData, FiniteElementsResponse response)
        {
            if (beamData.NumberOfElements < 1)
            {
                response.AddError(ErrorCode.BeamRequestData, $"Number of elements: {beamData.NumberOfElements} must be greather than or equals to 1.");
            }

            if (true)
            {

            }

            if (!response.Success)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
