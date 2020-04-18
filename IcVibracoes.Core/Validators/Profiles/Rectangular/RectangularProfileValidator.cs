using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.Profiles.Rectangular
{
    public class RectangularProfileValidator : ProfileValidator<RectangularProfile>, IRectangularProfileValidator
    {
        public override Task<bool> Execute(RectangularProfile profile, FiniteElementsResponse response)
        {
            return Task.FromResult(true);
        }
    }
}
