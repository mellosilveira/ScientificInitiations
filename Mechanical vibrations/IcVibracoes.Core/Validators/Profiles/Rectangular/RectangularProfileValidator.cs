using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;

namespace IcVibracoes.Core.Validators.Profiles.Rectangular
{
    /// <summary>
    /// It's responsible to validate a rectangular profile.
    /// </summary>
    public class RectangularProfileValidator : ProfileValidator<RectangularProfile>, IRectangularProfileValidator
    {
        /// <summary>
        /// This method validates the rectangular profile used in the analysis.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="response"></param>
        /// <returns>True, if the values in the rectangular profile can be used in the analysis. False, otherwise.</returns>
        public override bool Execute(RectangularProfile profile, FiniteElementResponse response)
        {
            if (base.Execute(profile, response) == false)
            {
                return false;
            }

            if (profile.Area == 0 && profile.MomentOfInertia == 0 && profile.Height == 0 && profile.Width == 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError,
                    $"Some parameter must be informed. If height and width are informed, Area and Moment of Inertia cannot be informed. If Area and Moment of Inertia are informed, height and width cannot be informed.");

                return false;
            }

            if (profile.Height > 0 && profile.Width > 0)
            {
                if (profile.Area != null || profile.MomentOfInertia != null)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When heigth and width are informed, Area: {profile.Area} and Moment of Inertia: {profile.MomentOfInertia} cannot be informed.");

                    return false;
                }

                if (profile.Thickness < 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Thickness cannot be less than zero.");

                    return false;
                }

                return true;
            }
            else if (profile.Height < 0 || profile.Width < 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid value to heigth: {profile.Height} and widht: {profile.Width}. Height and width must be greater than zero");

                return false;
            }

            if (profile.Area > 0 && profile.MomentOfInertia > 0)
            {
                if (profile.Height != 0 && profile.Width != 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When Area and Moment of Inertia are informed, heigth: {profile.Height} and width: {profile.Width} must be zero.");

                    return false;
                }

                if (profile.Thickness != 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"When Area and Moment of Inertia are informed, thickness: {profile.Thickness} must be zero.");
                }

                return true;
            }
            else
            {
                response.AddError(OperationErrorCode.RequestValidationError,
                    $"Area: {profile.Area} and Moment of Inertia: {profile.MomentOfInertia} must be greater than zero.");

                return false;
            }
        }
    }
}
