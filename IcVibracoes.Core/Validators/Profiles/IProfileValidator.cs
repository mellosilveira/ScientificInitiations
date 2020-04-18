using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
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
        Task<bool> Execute(TProfile profile, FiniteElementsResponse response);
    }
}
