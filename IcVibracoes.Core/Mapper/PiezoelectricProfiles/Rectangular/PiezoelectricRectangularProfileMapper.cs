using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper.PiezoelectricProfiles.Rectangular
{
    /// <summary>
    /// It's responsible to build the piezoelectric rectangular profile.
    /// </summary>
    public class PiezoelectricRectangularProfileMapper : PiezoelectricProfileMapper<RectangularProfile>, IPiezoelectricRectangularProfileMapper
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly ICalculateGeometricProperty _calculateGeometricProperty;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateGeometricProperty"></param>
        public PiezoelectricRectangularProfileMapper(
            IArrayOperation arrayOperation,
            ICalculateGeometricProperty calculateGeometricProperty)
        {
            this._arrayOperation = arrayOperation;
            this._calculateGeometricProperty = calculateGeometricProperty;
        }

        /// <summary>
        /// Method to build the piezoelectric rectangular profile.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="beamProfile"></param>
        /// <param name="numberOfPiezoelectricsPerElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<GeometricProperty> Execute(RectangularProfile piezoelectricProfile, RectangularProfile beamProfile, uint numberOfPiezoelectricsPerElements, uint[] elementsWithPiezoelectric, uint numberOfElements)
        {
            GeometricProperty geometricProperty = new GeometricProperty();

            double uniqueArea = await this._calculateGeometricProperty.CalculateArea(piezoelectricProfile.Height, piezoelectricProfile.Width, null);
            double area = uniqueArea * numberOfPiezoelectricsPerElements;
            
            double momentOfInertia = await this._calculateGeometricProperty.CalculatePiezoelectricMomentOfInertia(piezoelectricProfile.Height, piezoelectricProfile.Width, beamProfile.Height, numberOfPiezoelectricsPerElements);

            geometricProperty.Area = await this._arrayOperation.CreateVector(area, numberOfElements, elementsWithPiezoelectric, nameof(area));
            geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(momentOfInertia, numberOfElements, elementsWithPiezoelectric, nameof(momentOfInertia));

            return geometricProperty;
        }
    }
}
