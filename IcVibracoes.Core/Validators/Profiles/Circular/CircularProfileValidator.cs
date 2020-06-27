using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.Profiles.Circular
{
    public class CircularProfileValidator : ProfileValidator<CircularProfile>, ICircularProfileValidator
    {
        public override async Task<bool> Execute(CircularProfile profile, FiniteElementResponse response)
        {
            bool profileIsValid = await base.Execute(profile, response).ConfigureAwait(false);

            if (profile.Area == default && profile.MomentOfInertia == default && profile.Diameter == default)
            {
                response.AddError(OperationErrorCode.RequestValidationError,
                    $"Some parameter must be passed. If diameter is passed, area and Moment of Inertia cannot be passed. If area and Moment of Inertia are passed, diameter cannot be passed.");

                profileIsValid = false;

                return profileIsValid;
            }

            if (profile.Diameter > 0)
            {
                if (profile.Area != default || profile.MomentOfInertia != default)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When diameter is passed, area: {profile.Area} or Moment of Inertia: {profile.MomentOfInertia} must not be passed.");

                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
            else
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid value to diameter: {profile.Diameter}. Diameter must be greather than zero");
            }

            if (profile.Area > 0 && profile.MomentOfInertia > 0)
            {
                if (profile.Diameter != default)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When area and Moment of Inertia are passed, diameter: {profile.Diameter} must not be passed.");

                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
            else
            {
                response.AddError(OperationErrorCode.RequestValidationError,
                    $"Area: {profile.Area} and Moment of Inertia: {profile.MomentOfInertia} must be greather than zero.");

                return Task.FromResult(false);
            }
        }
    }
}
