using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Test.Helper;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.GeometricProperties.Rectangular
{
    public class RectangularGeometricPropertyTest : GeometricPropertyTest<RectangularProfile>
    {
        public RectangularGeometricPropertyTest()
        {
            base._calculator = new RectangularGeometricProperty();

            base._beamProfileWithThickness = GeometricPropertyModel.RectangularBeamProfileWithThickness;

            base._beamProfileWithoutThickness = GeometricPropertyModel.RectangularBeamProfileWithoutThickness;

            base._piezoelectricProfile = GeometricPropertyModel.RectangularPiezoelectricProfile;

            base._areaWithThickness = GeometricPropertyModel.RectangularAreaWithThickness;
            base._areaWithoutThickness = GeometricPropertyModel.RectangularAreaWithoutThickness;
            base._piezoelectricArea = GeometricPropertyModel.RectangularPiezoelectricArea;

            base._momentOfInertiaWithThickness = GeometricPropertyModel.RectangularMomentOfInertiaWithThickness;
            base._momentOfInertiaWithoutThickness = GeometricPropertyModel.RectangularMomentOfInertiaWithoutThickness;
            base._piezoelectricMomentOfInertia = GeometricPropertyModel.RectangularPiezoelectricMomentOfInertia;

            base._areaPrecision = GeometricPropertyModel.RectangularAreaPrecision;
            base._momentOfInertiaPrecision = GeometricPropertyModel.RectangularMomentOfInertiaPrecision;
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricArea | When: Execute. | Given: Valid parameters. | Should: Calculate correctly.")]
        public void CalculatePiezoelectricArea_Should_CalculateCorrectly()
        {
            // Arrange
            var result = base._calculator.CalculatePiezoelectricArea(this._piezoelectricProfile, this._numberOfElements, this._elementsWithPiezoelectric, this._numberOfPiezoelectricPerElement);

            // Assert
            result.ShouldBeBeApproximately(this._piezoelectricArea, this._areaPrecision);
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricMomentOfInertia | When: Execute. | Given: Valid parameters. | Should: Calculate correctly.")]
        public void CalculatePiezoelectricMomentOfInertia_Should_CalculateCorrectly()
        {
            // Arrange
            var result = base._calculator.CalculatePiezoelectricMomentOfInertia(this._piezoelectricProfile, this._beamProfileWithThickness, this._numberOfElements, this._elementsWithPiezoelectric, this._numberOfPiezoelectricPerElement);

            // Assert
            result.ShouldBeBeApproximately(this._piezoelectricMomentOfInertia, this._momentOfInertiaPrecision);
        }
    }
}
