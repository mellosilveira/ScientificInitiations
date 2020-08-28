using FluentAssertions;
using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam;
using IcVibracoes.Core.Validators.Profiles;
using Moq;
using System;
using Xunit;

namespace IcVibracoes.Test.Core.Operations.CalculateVibration.FiniteElement.Beam
{
    public class CalculateBeamVibrationTest<TProfile>
        where TProfile : Profile, new()
    {
        protected Mock<CalculateBeamVibration<TProfile>> _operationMock;

        private readonly Mock<IGeometricProperty<TProfile>> _geometricPropertyMock;
        private readonly Mock<IMappingResolver> _mappingResolverMock;
        private readonly Mock<IBeamMainMatrix<TProfile>> _mainMatrixMock;
        private readonly Mock<IProfileValidator<TProfile>> _profileValidatorMock;
        private readonly Mock<ITime> _timeMock;
        private readonly Mock<INaturalFrequency> _naturalFrequencyMock;

        public CalculateBeamVibrationTest()
        {
            this._geometricPropertyMock = new Mock<IGeometricProperty<TProfile>>();
            
            this._mappingResolverMock = new Mock<IMappingResolver>();
            
            this._mainMatrixMock = new Mock<IBeamMainMatrix<TProfile>>();
            
            this._profileValidatorMock = new Mock<IProfileValidator<TProfile>>();
            
            this._timeMock = new Mock<ITime>();
            
            this._naturalFrequencyMock = new Mock<INaturalFrequency>();
            
            this._operationMock = new Mock<CalculateBeamVibration<TProfile>>(
                this._geometricPropertyMock.Object,
                this._mappingResolverMock.Object,
                this._mainMatrixMock.Object,
                this._profileValidatorMock.Object,
                this._timeMock.Object,
                this._naturalFrequencyMock.Object);
        }

        [Fact(DisplayName = @"Feature: CalculateBeamVibration | When: Instantiate class. | Given: Null GeometricProperty. | Should: Throw argument null exception.")]
        public void CalculateBeamVibration_NullGeometricProperty_ShouldThrowArgumentNullException()
        {
            this.TestWhenInstantiateClassShouldThrowArgumentNullException(
                null,
                this._mappingResolverMock.Object,
                this._mainMatrixMock.Object,
                this._profileValidatorMock.Object,
                this._timeMock.Object,
                this._naturalFrequencyMock.Object);
        }

        private void TestWhenInstantiateClassShouldThrowArgumentNullException(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamMainMatrix<TProfile> mainMatrix,
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
        {
            // Act
            Action action = () => this._operationMock = new Mock<CalculateBeamVibration<TProfile>>(
                geometricProperty, mappingResolver, mainMatrix, profileValidator, time, naturalFrequency);

            // Assert
            action.Should().ThrowExactly<ArgumentNullException>();
        }
    }
}
