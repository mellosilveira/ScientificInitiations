using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public abstract class BeamMainMatrixTest<T, TProfile>
        where T : BeamMainMatrix<TProfile>, new()
        where TProfile : Profile, new()
    {
        protected double _beamArea;
        protected double _beamMomentOfInertia;

        protected double[,] _massMatrix;
        protected double[,] _elementMassMatrix;
        protected double[,] _stiffnessMatrix;
        protected double[,] _elementStiffnessMatrix;
        protected double[,] _dampingMatrix;
        protected double _precision;

        protected readonly bool[] _boundaryConditionsVector;
        protected readonly T _operation;
        protected readonly double _elementLength;
        protected readonly double[] _forceVector;

        protected Beam<TProfile> _beam;

        protected const int numberOfElements = 2;
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        protected const int degreesFreedomMaximum = 6;

        public BeamMainMatrixTest()
        {
            this._operation = new T();
            this._precision = 1e-6;

            this._elementLength = 0.5;

            this._forceVector = new double[degreesFreedomMaximum] { 0, 0, 100, 0, 0, 0 };

            this._boundaryConditionsVector = new bool[degreesFreedomMaximum] { false, true, true, true, false, true };
        }

        [Fact(DisplayName = @"Feature: CalculateElementMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementMass_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.CalculateElementMass(this._beamArea, this._beam.Material.SpecificMass, this._elementLength);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(_elementMassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMass_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.CalculateMass(this._beam, degreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(_massMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateElementStiffness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementStiffness_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.CalculateElementStiffness(this._beamMomentOfInertia, this._beam.Material.YoungModulus, this._elementLength);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(_elementStiffnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateStiffness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateStiffness_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.CalculateStiffness(this._beam, degreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(_stiffnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateDamping | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateDamping_Should_ExecuteCorrectly()
        {
            // Arrange
            this._beam = new Beam<TProfile>
            {
                FirstFastening = new Pinned(),
                Forces = this._forceVector,
                GeometricProperty = new GeometricProperty
                {
                    Area = new double[numberOfElements] { this._beamArea, this._beamArea },
                    MomentOfInertia = new double[numberOfElements] { this._beamMomentOfInertia, this._beamMomentOfInertia }
                },
                LastFastening = new Pinned(),
                Length = this._elementLength * numberOfElements,
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                Profile = new TProfile()
            };

            double[,] mass = await this._operation.CalculateMass(this._beam, degreesFreedomMaximum);
            double[,] stiffness = await this._operation.CalculateStiffness(this._beam, degreesFreedomMaximum);

            this._precision = 5e-3;

            // Act 
            var result = await this._operation.CalculateDamping(mass, stiffness);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(_dampingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateBondaryCondition | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateBondaryCondition_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateBondaryCondition(this._beam.FirstFastening, this._beam.LastFastening, degreesFreedomMaximum);

            // Assert
            result.Should().BeEquivalentTo(this._boundaryConditionsVector);
        }
    }
}
