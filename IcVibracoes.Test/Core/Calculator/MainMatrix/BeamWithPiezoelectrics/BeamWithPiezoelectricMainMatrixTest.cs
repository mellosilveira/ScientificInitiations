using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.BeamWithPiezoelectrics
{
    public class RectangularBeamWithPiezoelectricMainMatrixTest
    {
        private const int numberOfPiezoelectrics = 1;
        private const int numberOfElements = 2;
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int degreesFreedomMaximum = 6;
        // Piezoelectric Degrees Freedom Maximum = Number of Nodes = Number of Elements + 1 
        private const int piezoelectricDegreesFreedomMaximum = 3;

        private readonly IArrayOperation _arrayOperation;
        private readonly RectangularBeamWithPiezoelectricMainMatrix _operation;
        private readonly BeamWithPiezoelectric<RectangularProfile> _beamWithPiezoelectric;
        private readonly double _elementLength;
        private readonly bool[] _piezoelectricBoundaryConditions;

        private readonly double _precision;
        private readonly double _piezoelectricArea;
        private readonly double _piezoelectricMomentOfInertia;

        private readonly double[,] _massMatrix;
        private readonly double[,] _equivalentMassMatrix;

        private readonly double[,] _piezoelectricElementHardnessMatrix;
        private readonly double[,] _hardnessMatrix;
        private readonly double[,] _equivalentHardnessMatrix;

        private readonly double[,] _piezoelectricElementElectromechanicalCouplingMatrix;
        private readonly double[,] _piezoelectricElectromechanicalCouplingMatrix;
        private readonly double[,] _piezoelectricCapacitanceMatrix;
        private readonly double[,] _elementPiezoelectricCapacitanceMatrix;

        public RectangularBeamWithPiezoelectricMainMatrixTest()
        {
            this._piezoelectricArea = 6.675E-6;
            this._piezoelectricMomentOfInertia = 3.5701411E-11;
            this._precision = 1e-15;

            this._arrayOperation = new ArrayOperation();

            this._operation = new RectangularBeamWithPiezoelectricMainMatrix(this._arrayOperation);

            this._elementLength = 0.5;

            this._beamWithPiezoelectric = new BeamWithPiezoelectric<RectangularProfile>
            {
                DielectricConstant = 7.33e-9,
                DielectricPermissiveness = 30.705,
                ElasticityConstant = 1.076e11,
                ElectricalCharge = new double[numberOfPiezoelectrics] { 0 },
                ElementsWithPiezoelectric = new uint[numberOfPiezoelectrics] { 2 },
                FirstFastening = new Pinned(),
                Forces = new double[degreesFreedomMaximum] { 0, 0, 100, 0, 0, 0 },
                GeometricProperty = new GeometricProperty
                {
                    // Beam profile: height = 3e-3, width = 25e-3.
                    Area = new double[numberOfElements] { 7.5E-05, 7.5E-05 },
                    MomentOfInertia = new double[numberOfElements] { 5.625E-11, 5.625E-11 }
                },
                LastFastening = new Pinned(),
                Length = this._elementLength * numberOfElements,
                Material = new Aluminum(),
                NumberOfElements = numberOfElements,
                NumberOfPiezoelectricPerElements = 2,
                PiezoelectricConstant = 190e-12,
                PiezoelectricGeometricProperty = new GeometricProperty
                {
                    // Piezoelectric profile:  height = 0.267e-3, width = 25e-3.
                    Area = new double[numberOfElements] { 0, this._piezoelectricArea },
                    MomentOfInertia = new double[numberOfElements] { 0, this._piezoelectricMomentOfInertia }
                },
                PiezoelectricProfile = new RectangularProfile(),
                PiezoelectricSpecificMass = 7650,
                Profile = new RectangularProfile(),
                PiezoelectricYoungModulus = 63e9
            };

            this._piezoelectricBoundaryConditions = new bool[degreesFreedomMaximum / 2] { false, true, true };
        }

        [Fact(DisplayName = @"Feature: CalculateMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMass_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMass(this._beamWithPiezoelectric, degreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._massMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateHardness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateHardness(this._beamWithPiezoelectric, degreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._hardnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElementHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElementHardness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricElementHardness(this._beamWithPiezoelectric.ElasticityConstant, this._piezoelectricMomentOfInertia, this._elementLength);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElementHardnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElectromechanicalCoupling | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElectromechanicalCoupling_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricElectromechanicalCoupling(this._beamWithPiezoelectric, degreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElectromechanicalCouplingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElementElectromechanicalCoupling | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElementElectromechanicalCoupling_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricElementElectromechanicalCoupling(this._beamWithPiezoelectric);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElementElectromechanicalCouplingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricCapacitance | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricCapacitance_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricCapacitance(this._beamWithPiezoelectric);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricCapacitanceMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateElementPiezoelectricCapacitance | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementPiezoelectricCapacitance_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateElementPiezoelectricCapacitance(this._beamWithPiezoelectric, elementIndex: 1);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._elementPiezoelectricCapacitanceMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentMass_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateEquivalentMass(this._massMatrix, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._equivalentMassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentHardness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateEquivalentHardness(this._hardnessMatrix, this._piezoelectricElectromechanicalCouplingMatrix, this._piezoelectricCapacitanceMatrix, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._equivalentHardnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricBondaryCondition | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricBondaryCondition_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricBondaryCondition(this._beamWithPiezoelectric.NumberOfElements, this._beamWithPiezoelectric.ElementsWithPiezoelectric);

            // Assert
            result.Should().BeEquivalentTo(this._piezoelectricBoundaryConditions);
        }
    }
}
