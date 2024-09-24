﻿using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
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
        
        private double _precision;
        private readonly uint _numberOfPiezoelectricsPerElement;
        private readonly double _piezoelectricArea;
        private readonly double _piezoelectricMomentOfInertia;

        private readonly double[,] _piezoelectricElementMassMatrix;
        private readonly double[,] _massMatrix;
        private readonly double[,] _equivalentMassMatrix;

        private readonly double[,] _piezoelectricElementStiffnessMatrix;
        private readonly double[,] _stiffnessMatrix;
        private readonly double[,] _equivalentStiffnessMatrix;

        private readonly double[,] _piezoelectricElementElectromechanicalCouplingMatrix;
        private readonly double[,] _piezoelectricElectromechanicalCouplingMatrix;
        private readonly double[,] _elementPiezoelectricCapacitanceMatrix;
        private readonly double[,] _piezoelectricCapacitanceMatrix;

        private readonly double[,] _damping;

        public RectangularBeamWithPiezoelectricMainMatrixTest()
        {
            this._numberOfPiezoelectricsPerElement = 2;
            // Area and Moment of Inertia to piezoelectric were calculate manualy.
            this._piezoelectricArea = this._numberOfPiezoelectricsPerElement * 6.675E-6;
            this._piezoelectricMomentOfInertia = 3.5701411E-11;
            this._precision = 1e-6;

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
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                NumberOfPiezoelectricPerElements = 2,
                PiezoelectricConstant = 190e-12,
                PiezoelectricGeometricProperty = new GeometricProperty
                {
                    // Piezoelectric profile:  height = 0.267e-3, width = 25e-3.
                    Area = new double[numberOfElements] { 0, this._piezoelectricArea },
                    MomentOfInertia = new double[numberOfElements] { 0, this._piezoelectricMomentOfInertia }
                },
                PiezoelectricProfile = new RectangularProfile
                {
                    Height = 0.267e-3,
                    Width = 25e-3
                },
                PiezoelectricSpecificMass = 7650,
                Profile = new RectangularProfile
                {
                    Height = 3e-3,
                    Width = 25e-3
                },
                PiezoelectricYoungModulus = 63e9
            };

            this._piezoelectricElementMassMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement]
            {
                { 0.018967, 0.001337, 0.006565, -0.000790 },
                { 0.001337, 0.000122, 0.000790, -0.000091 },
                { 0.006565, 0.000790, 0.018967, -0.001337 },
                { -0.000790, -0.000091, -0.001337, 0.000122 }
            };

            this._massMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum]
            {
                { 0.109339, 0.007710, 0.037848, -0.004556, 0.000000, 0.000000 },
                { 0.007710, 0.000701, 0.004556, -0.000526, 0.000000, 0.000000 },
                { 0.037848, 0.004556, 0.237645, 0.001337, 0.044414, -0.005346 },
                { -0.004556, -0.000526, 0.001337, 0.001523, 0.005346, -0.000617 },
                { 0.000000, 0.000000, 0.044414, 0.005346, 0.128306, -0.009047 },
                { 0.000000, 0.000000, -0.005346, -0.000617, -0.009047, 0.000822 }
            };

            this._piezoelectricElementStiffnessMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement]
            {
                { 368.781296, 92.195324, -368.781296, 92.195324 },
                { 92.195324, 30.731775, -92.195324, 15.365887 },
                { -368.781296, -92.195324, 368.781296, -92.195324 },
                { 92.195324, 15.365887, -92.195324, 30.731775 }
            };

            this._stiffnessMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum]
            {
                { 1080.000000, 270.000000, -1080.000000, 270.000000, 0.000000, 0.000000 },
                { 270.000000, 90.000000, -270.000000, 45.000000, 0.000000, 0.000000 },
                { -1080.000000, -270.000000, 2528.781296, 92.195324, -1448.781296, 362.195324 },
                { 270.000000, 45.000000, 92.195324, 210.731775, -362.195324, 60.365887 },
                { 0.000000, 0.000000, -1448.781296, -362.195324, 1448.781296, -362.195324 },
                { 0.000000, 0.000000, 362.195324, 60.365887, -362.195324, 120.731775 }
            };

            this._piezoelectricElementElectromechanicalCouplingMatrix = new double[Constant.DegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement]
            {
                { 0, 0 },
                { 1.60557e-07, -1.60557e-07 },
                { 0, -1.60557e-07 },
                { -1.60557e-07, 1.60557e-07 }
            };

            this._piezoelectricElectromechanicalCouplingMatrix = new double[degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum]
            {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 1.60557e-07, -1.60557e-07 },
                { 0, 0, -1.60557e-07 },
                { 0, -1.60557e-07, 1.60557e-07 }
            };

            this._elementPiezoelectricCapacitanceMatrix = new double[Constant.PiezoelectricDegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement]
            {
                { -6.8633e-07, 6.8633e-07 },
                { 6.8633e-07, -6.8633e-07 }
            };

            this._piezoelectricCapacitanceMatrix = new double[piezoelectricDegreesFreedomMaximum, piezoelectricDegreesFreedomMaximum]
            {
                { 0, 0, 0 },
                { 0, -6.8633e-07, 6.8633e-07 },
                { 0, 6.8633e-07, -6.8633e-07 }
            };

            this._equivalentMassMatrix = new double[degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum, degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum]
            {
                { 0.109339, 0.00770982, 0.0378482, -0.0045558, 0, 0, 0, 0, 0 },
                { 0.00770982, 0.000700893, 0.0045558, -0.00052567, 0, 0, 0, 0, 0 },
                { 0.0378482, 0.0045558, 0.237645, 0.00133738, 0.0444136, -0.00534608, 0, 0, 0 },
                { -0.0045558, -0.00052567, 0.00133738, 0.00152337, 0.00534608, -0.000616855, 0, 0, 0 },
                { 0, 0, 0.0444136, 0.00534608, 0.128306, -0.00904721, 0, 0, 0 },
                { 0, 0, -0.00534608, -0.000616855, -0.00904721, 0.000822473, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            
            this._equivalentStiffnessMatrix = new double[degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum, degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum]
            { 
                { 1080, 270, -1080, 270, 0, 0, 0, 0, 0 },
                { 270, 90, -270, 45, 0, 0, 0, 0, 0 },
                { -1080, -270, 2528.78, 92.1953, -1448.78, 362.195, 0, 0, 0 },
                { 270, 45, 92.1953, 210.732, -362.195, 60.3659, 0, 1.60557e-07, -1.60557e-07 },
                { 0, 0, -1448.78, -362.195, 1448.78, -362.195, 0, 0, -1.60557e-07 },
                { 0, 0, 362.195, 60.3659, -362.195, 120.732, 0, -1.60557e-07, 1.60557e-07 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1.60557e-07, 0, -1.60557e-07, 0, -6.8633e-07, 6.8633e-07 },
                { 0, 0, 0, -1.60557e-07, -1.60557e-07, 1.60557e-07, 0, 6.8633e-07, -6.8633e-07 }
            };

            this._damping = new double[degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum, degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum]
            {
                { 0.00108, 0.00027, -0.00108, 0.00027, 0, 0, 0, 0, 0 },
                { 0.00027, 9e-05, -0.00027, 4.5e-05, 0, 0, 0, 0, 0 },
                { -0.00108, -0.00027, 0.00252878, 9.21953e-05, -0.00144878, 0.000362195, 0, 0, 0 },
                { 0.00027, 4.5e-05, 9.21953e-05, 0.000210732, -0.000362195, 6.03659e-05, 0, 1.60557e-13, -1.60557e-13 },
                { 0, 0, -0.00144878, -0.000362195, 0.00144878, -0.000362195, 0, 0, -1.60557e-13 },
                { 0, 0, 0.000362195, 6.03659e-05, -0.000362195, 0.000120732, 0, -1.60557e-13, 1.60557e-13 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1.60557e-13, 0, -1.60557e-13, 0, -6.8633e-13, 6.8633e-13 },
                { 0, 0, 0, -1.60557e-13, -1.60557e-13, 1.60557e-13, 0, 6.8633e-13, -6.8633e-13 }
            };

            this._piezoelectricBoundaryConditions = new bool[degreesFreedomMaximum / 2] { false, true, true };
        }

        [Fact(DisplayName = @"Feature: CalculateMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementMass_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateElementMass(
                this._beamWithPiezoelectric.PiezoelectricGeometricProperty.Area[1],
                this._beamWithPiezoelectric.PiezoelectricSpecificMass,
                this._elementLength).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElementMassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMass_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMass(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._massMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateStiffness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateStiffness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateStiffness(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._stiffnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElementStiffness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElementStiffness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricElementStiffness(this._beamWithPiezoelectric.ElasticityConstant, this._piezoelectricMomentOfInertia, this._elementLength).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.DegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElementStiffnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElectromechanicalCoupling | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElectromechanicalCoupling_Should_ExecuteCorrectly()
        {
            // Arrange
            this._precision = 1e-12;

            // Act
            var result = await this._operation.CalculatePiezoelectricElectromechanicalCoupling(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                for (int j = 0; j < piezoelectricDegreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElectromechanicalCouplingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricElementElectromechanicalCoupling | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricElementElectromechanicalCoupling_Should_ExecuteCorrectly()
        {
            // Arrange
            this._precision = 1e-12;

            // Act
            var result = await this._operation.CalculatePiezoelectricElementElectromechanicalCoupling(this._beamWithPiezoelectric).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < Constant.DegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.PiezoelectricDegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricElementElectromechanicalCouplingMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricCapacitance | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricCapacitance_Should_ExecuteCorrectly()
        {
            // Arrange
            this._precision = 1e-12;

            // Act
            var result = await this._operation.CalculatePiezoelectricCapacitance(this._beamWithPiezoelectric).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < piezoelectricDegreesFreedomMaximum; i++)
            {
                for (int j = 0; j < piezoelectricDegreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._piezoelectricCapacitanceMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateElementPiezoelectricCapacitance | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateElementPiezoelectricCapacitance_Should_ExecuteCorrectly()
        {
            // Arrange
            this._precision = 1e-12;

            // Act
            var result = await this._operation.CalculateElementPiezoelectricCapacitance(this._beamWithPiezoelectric, elementIndex: 1).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < Constant.PiezoelectricDegreesFreedomElement; i++)
            {
                for (int j = 0; j < Constant.PiezoelectricDegreesFreedomElement; j++)
                {
                    result[i, j].Should().BeApproximately(this._elementPiezoelectricCapacitanceMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentMass | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentMass_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateEquivalentMass(this._massMatrix, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._equivalentMassMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentStiffness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentStiffness_Should_ExecuteCorrectly()
        {
            // Arrange
            this._precision = 5e-3;

            // Act
            var result = await this._operation.CalculateEquivalentStiffness(this._stiffnessMatrix, this._piezoelectricElectromechanicalCouplingMatrix, this._piezoelectricCapacitanceMatrix, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._equivalentStiffnessMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateDamping | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateDamping_Should_ExecuteCorrectly()
        {
            // Arrange
            double[,] mass = await this._operation.CalculateMass(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);
            double[,] equivalentMass = await this._operation.CalculateEquivalentMass(mass, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            double[,] stiffness = await this._operation.CalculateStiffness(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);
            double[,] piezoelectricElectromechanicalCoupling = await this._operation.CalculatePiezoelectricElectromechanicalCoupling(this._beamWithPiezoelectric, degreesFreedomMaximum).ConfigureAwait(false);
            double[,] piezoelectricCapacitance = await this._operation.CalculatePiezoelectricCapacitance(this._beamWithPiezoelectric).ConfigureAwait(false);
            double[,] equivalentStiffness = await this._operation.CalculateEquivalentStiffness(stiffness, piezoelectricElectromechanicalCoupling, piezoelectricCapacitance, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            this._precision = 5e-3;

            // Act
            var result = await this._operation.CalculateDamping(equivalentMass, equivalentStiffness).ConfigureAwait(false);

            // Assert
            for (int i = 0; i < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; i++)
            {
                for (int j = 0; j < degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum; j++)
                {
                    result[i, j].Should().BeApproximately(this._damping[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricBondaryCondition | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricBondaryCondition_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricBondaryCondition(this._beamWithPiezoelectric.NumberOfElements, this._beamWithPiezoelectric.ElementsWithPiezoelectric).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(this._piezoelectricBoundaryConditions);
        }
    }
}
