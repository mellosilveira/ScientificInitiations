using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public abstract class BeamMainMatrixTest<TProfile>
        where TProfile : Profile, new()
    {
        protected double Area { get; set; }
        protected double MomentOfInertia { get; set; }

        protected double[,] MassMatrix { get; set; }
        protected double[,] ElementMassMatrix { get; set; }
        protected double[,] HardnessMatrix { get; set; }
        protected double[,] ElementHardnessMatrix { get; set; }
        protected double[,] DampingMatrix { get; set; }

        private readonly bool[] _boundaryConditionsVector;
        private readonly Mock<BeamMainMatrix<TProfile>> _operationMock;
        private readonly Beam<TProfile> _beam;
        private readonly double _elementLength;
        private readonly double[] _forceVector;
        private readonly double _precision;

        private const int numberOfElements = 2;
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int degreesFreedomMaximum = 6;

        public BeamMainMatrixTest()
        {
            this._operationMock = new Mock<BeamMainMatrix<TProfile>>();
            this._precision = 1e-15;

            this._elementLength = 0.5;

            this._forceVector = new double[degreesFreedomMaximum] { 0, 0, 100, 0, 0, 0 };

            this._boundaryConditionsVector = new bool[degreesFreedomMaximum] { true, false, true, true, true, false };

            this._beam = new Beam<TProfile>
            {
                FirstFastening = new Pinned(),
                Forces = this._forceVector,
                GeometricProperty = new GeometricProperty
                {
                    Area = new double[numberOfElements] { this.Area, this.Area },
                    MomentOfInertia = new double[numberOfElements] { this.MomentOfInertia, this.MomentOfInertia }
                },
                LastFastening = new Pinned(),
                Length = this._elementLength * numberOfElements,
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                Profile = new TProfile()
            };
        }

        [Fact(DisplayName = @"Feature: CalculateElementMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementMass_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operationMock.Object.CalculateElementMass(this.Area, this._beam.Material.SpecificMass, this._elementLength);

            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(ElementMassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMass_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operationMock.Object.CalculateMass(this._beam, degreesFreedomMaximum);

            // Assert
            this._operationMock.Verify(eoq => eoq.CalculateElementMass(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Exactly(numberOfElements));

            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(MassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateElementHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementHardness_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operationMock.Object.CalculateElementHardness(this.MomentOfInertia, this._beam.Material.YoungModulus, this._elementLength);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(ElementHardnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateHardness_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operationMock.Object.CalculateHardness(this._beam, degreesFreedomMaximum);

            // Assert
            this._operationMock.Verify(eoq => eoq.CalculateElementHardness(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Exactly(numberOfElements));

            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(HardnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateDamping | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateDamping_Should_ExecuteCorrectly()
        {
            // Arrange
            double[,] mass = await this._operationMock.Object.CalculateMass(this._beam, degreesFreedomMaximum);
            double[,] hardness = await this._operationMock.Object.CalculateHardness(this._beam, degreesFreedomMaximum);

            // Act 
            var result = await this._operationMock.Object.CalculateDamping(mass, hardness);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(DampingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateBondaryCondition | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateBondaryCondition_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operationMock.Object.CalculateBondaryCondition(this._beam.FirstFastening, this._beam.LastFastening, degreesFreedomMaximum);

            // Assert
            result.Should().BeEquivalentTo(this._boundaryConditionsVector);
        }
    }
}
