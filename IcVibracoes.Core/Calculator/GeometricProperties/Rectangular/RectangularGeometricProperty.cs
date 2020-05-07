using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations.ArrayOperations;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.GeometricProperties.Rectangular
{
    /// <summary>
    /// It's responsible to calculate any geometric property for a rectangular profile.
    /// </summary>
    public class RectangularGeometricProperty : GeometricProperty<RectangularProfile>, IRectangularGeometricProperty
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public RectangularGeometricProperty(IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// This method calculates the vector with the beam or piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateArea(RectangularProfile profile, uint numberOfElements)
        {
            double area;

            if (profile.Thickness == null)
            {
                area = profile.Height * profile.Width;
            }
            else
            {
                area = (profile.Height * profile.Width) - ((profile.Height - 2 * profile.Thickness.Value) * (profile.Width - 2 * profile.Thickness.Value));
            }

            return await this._arrayOperation.CreateVector(area, numberOfElements).ConfigureAwait(false);
        }

        /// <summary>
        /// This method calculates the vector with the beam moment of inertia.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateMomentOfInertia(RectangularProfile profile, uint numberOfElements)
        {
            double momentOfInertia;

            if (profile.Thickness == null)
            {
                momentOfInertia = Math.Pow(profile.Height, 3) * profile.Width / 12;
            }
            else
            {
                momentOfInertia = (Math.Pow(profile.Height, 3) * profile.Width - (Math.Pow(profile.Height - 2 * profile.Thickness.Value, 3) * (profile.Width - 2 * profile.Thickness.Value))) / 12;
            }

            return await this._arrayOperation.CreateVector(momentOfInertia, numberOfElements).ConfigureAwait(false);
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="numberOfPiezoelectricPerElement"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculatePiezoelectricArea(RectangularProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricPerElement)
        {
            double area;

            if (profile.Thickness == null)
            {
                area = profile.Height * profile.Width;
            }
            else
            {
                area = (profile.Height * profile.Width) - ((profile.Height - 2 * profile.Thickness.Value) * (profile.Width - 2 * profile.Thickness.Value));
            }

            area *= numberOfPiezoelectricPerElement;

            return await this._arrayOperation.CreateVector(area, numberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric moment of inertia.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricsPerElement"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculatePiezoelectricMomentOfInertia(RectangularProfile piezoelectricProfile, RectangularProfile beamProfile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricsPerElement)
        {
            double momentOfInertia;

            if (numberOfPiezoelectricsPerElement <= 2 || numberOfPiezoelectricsPerElement > 0)
            {
                momentOfInertia = numberOfPiezoelectricsPerElement * ((Math.Pow(piezoelectricProfile.Height, 3) * piezoelectricProfile.Width / 12) + (piezoelectricProfile.Height * piezoelectricProfile.Width * Math.Pow((beamProfile.Height + piezoelectricProfile.Height) / 2, 2)));
            }
            else
            {
                throw new NotImplementedException($"Not implemented moment of inertia calculation to number of piezoelectric:{numberOfPiezoelectricsPerElement}.");
            }

            return await this._arrayOperation.CreateVector(momentOfInertia, numberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
        }
    }
}
