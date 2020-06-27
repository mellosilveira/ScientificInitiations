using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElement;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.Profiles
{
    /// <summary>
    /// It's responsible to validate any profile.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IProfileValidator<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// This method validates the profile used in the analysis.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="response"></param>
        /// <returns>True, if the values passed in the profile can be used in the analysis. False, otherwise.</returns>
        Task<bool> Execute(TProfile profile, FiniteElementResponse response);
    }
}
