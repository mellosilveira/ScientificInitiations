using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Test.Helper;
using System;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.GeometricProperties.Circular
{
    public class CircularGeometricPropertyTest : GeometricPropertyTest<CircularProfile>
    {
        public CircularGeometricPropertyTest()
        {
            base._calculator = new CircularGeometricProperty();

            base._beamProfileWithThickness = GeometricPropertyModel.CircularBeamProfileWithThickness;

            base._beamProfileWithoutThickness = GeometricPropertyModel.CircularBeamProfileWithoutThickness;

            base._piezoelectricProfile = GeometricPropertyModel.CircularPiezoelectricProfile;

            base._areaWithThickness = GeometricPropertyModel.CircularAreaWithThickness;
            base._areaWithoutThickness = GeometricPropertyModel.CircularAreaWithoutThickness;
            base._piezoelectricArea = GeometricPropertyModel.CircularPiezoelectricArea;

            base._momentOfInertiaWithThickness = GeometricPropertyModel.CircularMomentOfInertiaWithThickness;
            base._momentOfInertiaWithoutThickness = GeometricPropertyModel.CircularMomentOfInertiaWithoutThickness;
            base._piezoelectricMomentOfInertia = GeometricPropertyModel.CircularPiezoelectricMomentOfInertia;

            base._areaPrecision = GeometricPropertyModel.CircularAreaPrecision;
            base._momentOfInertiaPrecision = GeometricPropertyModel.CircularMomentOfInertiaPrecision;
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricArea | When: Execute. | Given: Valid parameters. | Should: Throw not implemented exception.")]
        public void CalculatePiezoelectricArea_Should_ThrowNotImplementedException()
        {
            // Arrange
            Action func = () => base._calculator.CalculatePiezoelectricArea(this._piezoelectricProfile, this._numberOfElements, this._elementsWithPiezoelectric, this._numberOfPiezoelectricPerElement);

            // Assert
            func.Should().Throw<NotImplementedException>();
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricMomentOfInertia | When: Execute. | Given: Valid parameters. | Should: Throw not implemented exception.")]
        public void CalculatePiezoelectricMomentOfInertia_Should_ThrowNotImplementedException()
        {
            // Arrange
            Action func = () => base._calculator.CalculatePiezoelectricMomentOfInertia(this._piezoelectricProfile, this._beamProfileWithThickness, this._numberOfElements, this._elementsWithPiezoelectric, this._numberOfPiezoelectricPerElement);

            // Assert
            func.Should().Throw<NotImplementedException>();
        }
    }
}
