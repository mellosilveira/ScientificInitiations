using IcVibracoes.Common.Profiles;
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
        public abstract Task<bool> Execute(TProfile profile, FiniteElementResponse response);
    }
}
