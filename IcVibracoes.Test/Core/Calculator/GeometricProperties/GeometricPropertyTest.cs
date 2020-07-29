using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.GeometricProperties
{
    public abstract class GeometricPropertyTest<TProfile>
        where TProfile : Profile, new()
    {
        protected GeometricProperty<TProfile> _calculator;

        protected TProfile _beamProfileWithThickness;
        protected TProfile _beamProfileWithoutThickness;
        protected TProfile _piezoelectricProfile;

        protected uint _numberOfElements;
        protected double[] _areaWithThickness;
        protected double[] _areaWithoutThickness;
        protected double[] _momentOfInertiaWithThickness;
        protected double[] _momentOfInertiaWithoutThickness;
        protected double[] _piezoelectricMomentOfInertia;
        protected double[] _piezoelectricArea;
        protected uint[] _elementsWithPiezoelectric;
        protected uint _numberOfPiezoelectricPerElement;

        protected double _areaPrecision;
        protected double _momentOfInertiaPrecision;

        public GeometricPropertyTest()
        {
            this._numberOfElements = 2;

            this._areaWithThickness = new double[] { };
            this._areaWithoutThickness = new double[] { };
            this._momentOfInertiaWithThickness = new double[] { };
            this._momentOfInertiaWithoutThickness = new double[] { };

            this._piezoelectricArea = new double[] { };
            this._piezoelectricMomentOfInertia = new double[] { };

            this._elementsWithPiezoelectric = new uint[] { 1, 2 };

            this._numberOfPiezoelectricPerElement = 2;
        }

        [Fact(DisplayName = @"Feature: CalculateArea | When: Execute. | Given: Profile with thickness. | Should: Calculate correctly.")]
        public async void CalculateArea_GivenProfileWithThickness_Should_CalculateCorrectly()
        {
            // Arrange
            var result = await this._calculator.CalculateArea(this._beamProfileWithThickness, this._numberOfElements).ConfigureAwait(false);

            // Assert
            result.ShouldBeBeApproximately(this._areaWithThickness, this._areaPrecision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | When: Execute. | Given: Profile with thickness. | Should: Calculate correctly.")]
        public async void CalculateMomentOfInertia_GivenProfileWithThickness_Should_CalculateCorrectly()
        {
            // Arrange
            var result = await this._calculator.CalculateMomentOfInertia(this._beamProfileWithThickness, this._numberOfElements).ConfigureAwait(false);

            // Assert
            result.ShouldBeBeApproximately(this._momentOfInertiaWithThickness, this._momentOfInertiaPrecision);
        }

        [Fact(DisplayName = @"Feature: CalculateArea | When: Execute. | Given: Profile without thickness. | Should: Calculate correctly.")]
        public async void CalculateArea_GivenProfileWithoutThickness_Should_CalculateCorrectly()
        {
            // Arrange
            var result = await this._calculator.CalculateArea(this._beamProfileWithoutThickness, this._numberOfElements).ConfigureAwait(false);

            // Assert
            result.ShouldBeBeApproximately(this._areaWithoutThickness, this._areaPrecision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | When: Execute. | Given: Profile without thickness. | Should: Calculate correctly.")]
        public async void CalculateMomentOfInertia_GivenProfileWithoutThickness_Should_CalculateCorrectly()
        {
            // Arrange
            var result = await this._calculator.CalculateMomentOfInertia(this._beamProfileWithoutThickness, this._numberOfElements).ConfigureAwait(false);

            // Assert
            result.ShouldBeBeApproximately(this._momentOfInertiaWithoutThickness, this._momentOfInertiaPrecision);
        }
    }
}
