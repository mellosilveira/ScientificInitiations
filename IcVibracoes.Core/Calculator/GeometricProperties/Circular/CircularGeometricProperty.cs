using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations.ArrayOperations;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.GeometricProperties.Circular
{
    /// <summary>
    /// It's responsible to calculate any geometric property for a circular profile.
    /// </summary>
    public class CircularGeometricProperty : GeometricProperty<CircularProfile>, ICircularGeometricProperty
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public CircularGeometricProperty(IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// This method calculates the vector with the beam area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateArea(CircularProfile profile, uint numberOfElements)
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

            return await this._arrayOperation.CreateVector(area, numberOfElements).ConfigureAwait(false);
        }

        /// <summary>
        /// This method calculates the vector with the beam moment of inertia.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateMomentOfInertia(CircularProfile profile, uint numberOfElements)
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

            return await this._arrayOperation.CreateVector(momentOfInertia, numberOfElements).ConfigureAwait(false);
        }

        /// <summary>
        /// This method calculates the vector with the piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="numberOfPiezoelectricPerElement"></param>
        /// <returns></returns>
        public override Task<double[]> CalculatePiezoelectricArea(CircularProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricPerElement)
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
        public override Task<double[]> CalculatePiezoelectricMomentOfInertia(CircularProfile piezoelectricProfile, CircularProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricsPerElement)
        {
            throw new NotImplementedException();
        }
    }
}
