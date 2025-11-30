using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;

namespace IcVibracoes.Core.Validators.Profiles.Circular
{
    /// <summary>
    /// It's responsible to validate a circular profile.
    /// </summary>
    public class CircularProfileValidator : ProfileValidator<CircularProfile>, ICircularProfileValidator
    {
        /// <summary>
        /// This method validates the circular profile used in the analysis.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="response"></param>
        /// <returns>True, if the values in the circular profile can be used in the analysis. False, otherwise.</returns>
        public override bool Execute(CircularProfile profile, FiniteElementResponse response)
        {
            if (base.Execute(profile, response) == false)
            {
                return false;
            }

            if (profile.Area == 0 && profile.MomentOfInertia == 0 && profile.Diameter == 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError,
                    "Some parameter must be informed. If diameter is informed, Area and Moment of Inertia cannot be informed. If Area and Moment of Inertia are informed, diameter cannot be informed.");

                return false;
            }

            if (profile.Diameter > 0)
            {
                if (profile.Area != 0 || profile.MomentOfInertia != 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When diameter is informed, Area: {profile.Area} and Moment of Inertia: {profile.MomentOfInertia} cannot be informed.");

                    return false;
                }

                if (profile.Thickness < 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Thickness cannot be less than zero.");

                    return false;
                }

                return true;
            }

            response.AddErrorIf(() => (profile.Diameter < 0), $"Invalid value to diameter: {profile.Diameter}. Diameter must be greater than zero");

            if (profile.Area > 0 && profile.MomentOfInertia > 0)
            {
                if (profile.Diameter != 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When Area and Moment of Inertia are informed, diameter: {profile.Diameter} and thickness: {profile.Thickness} cannot be informed.");

                    return false;
                }

                if (profile.Thickness != 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When Area and Moment of Inertia are informed, thickness: {profile.Thickness} must be zero.");
                }

                return true;
            }

            response.AddError(OperationErrorCode.RequestValidationError,
                $"Area: {profile.Area} and Moment of Inertia: {profile.MomentOfInertia} must be greater than zero.");

            return false;
        }
    }
}
