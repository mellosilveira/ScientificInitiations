using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.Profiles
{
    /// <summary>
    /// It's responsible to validate any profile.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class ProfileValidator<TProfile> : IProfileValidator<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// This method validates the profile used in the analysis.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="response"></param>
        /// <returns>True, if the values in the profile can be used in the analysis. False, otherwise.</returns>
        public virtual Task<bool> Execute(TProfile profile, FiniteElementResponse response)
        {
            if (profile == null)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Profile cannot be null.");

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
