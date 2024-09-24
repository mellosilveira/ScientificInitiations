﻿using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ExtensionMethods;
using System;

namespace IcVibracoes.Core.Calculator.GeometricProperties.Circular
{
    /// <summary>
    /// It's responsible to calculate any geometric property for a circular profile.
    /// </summary>
    public class CircularGeometricProperty : GeometricProperty<CircularProfile>, ICircularGeometricProperty
    {
        /// <summary>
        /// This method calculates the vector with the beam area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override double[] CalculateArea(CircularProfile profile, uint numberOfElements)
        {
            double area;

            if (profile.Thickness == null)
            {
                area = Math.PI * Math.Pow(profile.Diameter, 2) / 4;
            }
            else
            {
                area = (Math.PI / 4) * (Math.Pow(profile.Diameter, 2) - Math.Pow(profile.Diameter - 2 * profile.Thickness.Value, 2));
            }

            return ArrayFactory.CreateVector(area, numberOfElements);
        }

        /// <summary>
        /// This method calculates the vector with the beam moment of inertia.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override double[] CalculateMomentOfInertia(CircularProfile profile, uint numberOfElements)
        {
            double momentOfInertia;

            if (profile.Thickness == null)
            {
                momentOfInertia = Math.PI * Math.Pow(profile.Diameter, 4) / 64;
            }
            else
            {
                momentOfInertia = (Math.PI / 64) * (Math.Pow(profile.Diameter, 4) - Math.Pow(profile.Diameter - 2 * profile.Thickness.Value, 4));
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
        public override double[] CalculatePiezoelectricArea(CircularProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricPerElement)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric moment of inertia.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricsPerElement"></param>
        /// <returns></returns>
        public override double[] CalculatePiezoelectricMomentOfInertia(CircularProfile piezoelectricProfile, CircularProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricsPerElement)
        {
            throw new NotImplementedException();
        }
    }
}
