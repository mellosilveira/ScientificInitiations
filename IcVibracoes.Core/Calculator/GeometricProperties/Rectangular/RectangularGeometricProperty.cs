using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ExtensionMethods;
using System;

namespace IcVibracoes.Core.Calculator.GeometricProperties.Rectangular
{
    /// <summary>
    /// It's responsible to calculate any geometric property for a rectangular profile.
    /// </summary>
    public class RectangularGeometricProperty : GeometricProperty<RectangularProfile>, IRectangularGeometricProperty
    {
        /// <summary>
        /// This method calculates the vector with the beam or piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override double[] CalculateArea(RectangularProfile profile, uint numberOfElements)
        {
            double area = profile.Height * profile.Width;

            if (profile.Thickness != null)
            { 
                area = (profile.Height * profile.Width) - ((profile.Height - 2 * profile.Thickness.Value) * (profile.Width - 2 * profile.Thickness.Value));
            }

            return ArrayFactory.CreateVector(area, numberOfElements);
        }

        /// <summary>
        /// This method calculates the vector with the beam moment of inertia.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override double[] CalculateMomentOfInertia(RectangularProfile profile, uint numberOfElements)
        {
            double momentOfInertia = Math.Pow(profile.Height, 3) * profile.Width / 12;

            if (profile.Thickness != null)
            {
                momentOfInertia = (Math.Pow(profile.Height, 3) * profile.Width - (Math.Pow(profile.Height - 2 * profile.Thickness.Value, 3) * (profile.Width - 2 * profile.Thickness.Value))) / 12;
            }

            return ArrayFactory.CreateVector(momentOfInertia, numberOfElements);
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricPerElement"></param>
        /// <returns></returns>
        public override double[] CalculatePiezoelectricArea(RectangularProfile profile, uint numberOfElements,
            uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricPerElement)
        {
            double area = profile.Height * profile.Width;

            if (profile.Thickness != null)
            {
                area = (profile.Height * profile.Width) - ((profile.Height - 2 * profile.Thickness.Value) * (profile.Width - 2 * profile.Thickness.Value));
            }

            area *= numberOfPiezoelectricPerElement;

            return ArrayFactory.CreateVector(area, numberOfElements, elementsWithPiezoelectric);
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric moment of inertia.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="beamProfile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricsPerElement"></param>
        /// <returns></returns>
        public override double[] CalculatePiezoelectricMomentOfInertia(RectangularProfile piezoelectricProfile,
            RectangularProfile beamProfile, uint numberOfElements, uint[] elementsWithPiezoelectric,
            uint numberOfPiezoelectricsPerElement)
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

            return ArrayFactory.CreateVector(momentOfInertia, numberOfElements, elementsWithPiezoelectric);
        }
    }
}
