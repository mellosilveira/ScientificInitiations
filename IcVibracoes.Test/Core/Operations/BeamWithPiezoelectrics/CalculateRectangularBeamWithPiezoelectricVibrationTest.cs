using FluentAssertions;
using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.PiezoelectricProfiles.Rectangular;
using IcVibracoes.Core.Mapper.Profiles.Rectangular;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles.Rectangular;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;
using System.Collections.Generic;
using Xunit;

namespace IcVibracoes.Test.Core.Operations.BeamWithPiezoelectrics
{
    public class CalculateRectangularBeamWithPiezoelectricVibrationTest
    {
        private const int numberOfElements = 2;
        private const int numberOfElementsWithPiezoelectrics = 1;
        private const int piezoelectricDegreesFreedomMaximum = 3;
        private const int degreesFreedomMaximum = 6;
        private const int numberOfBoundaryConditionsTrue = 6;
        private const double elementLength = 0.5;

        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly INewmarkMethodValidator _newmarkMethodValidator;
        private readonly IArrayOperation _arrayOperation;
        private readonly INewmarkMethod _newmarkMethod;
        private readonly IMappingResolver _mappingResolver;
        private readonly IRectangularProfileValidator _profileValidator;
        private readonly IRectangularProfileMapper _profileMapper;
        private readonly ICalculateGeometricProperty _calculateGeometricProperty;
        private readonly IPiezoelectricRectangularProfileMapper _piezoelectricProfileMapper;
        private readonly IRectangularBeamWithPiezoelectricMainMatrix _mainMatrix;

        private readonly uint _numberOfPiezoelectricsPerElement;
        private readonly double _piezoelectricArea;
        private readonly double _piezoelectricMomentOfInertia;
        private readonly double[] _equivalentForce;
        private readonly double[,] _mass;
        private readonly double[,] _hardness;
        private readonly double[,] _damping;
        private readonly RectangularProfile _beamProfile;
        private readonly RectangularProfile _piezoelectricProfile;
        private readonly BeamWithPiezoelectric<RectangularProfile> _beamWithPiezoelectic;
        private readonly NewmarkMethodParameter _methodParameter;
        private readonly NewmarkMethodInput _newmarkMethodInput;
        private readonly CalculateRectangularBeamWithPiezoelectricVibration _operation;
        private readonly BeamWithPiezoelectricRequest<RectangularProfile> _request;

        private double _precision;

        public CalculateRectangularBeamWithPiezoelectricVibrationTest()
        {
            this._precision = 1e-16;

            this._beamProfile = new RectangularProfile
            {
                Height = 3e-3,
                Width = 25e-3
            };

            this._piezoelectricProfile = new RectangularProfile
            {
                Height = 0.267e-3,
                Width = 25e-3
            };

            this._numberOfPiezoelectricsPerElement = 2;
            // Area and Moment of Inertia to piezoelectric were calculate manualy.
            this._piezoelectricArea = this._numberOfPiezoelectricsPerElement * 6.675E-6;
            this._piezoelectricMomentOfInertia = 3.5701411E-11;

            this._arrayOperation = new ArrayOperation();
            this._newmarkMethodValidator = new NewmarkMethodValidator();
            this._auxiliarOperation = new AuxiliarOperation();

            this._newmarkMethod = new NewmarkMethod(
                this._arrayOperation,
                this._auxiliarOperation,
                this._newmarkMethodValidator);

            this._mappingResolver = new MappingResolver();
            this._profileValidator = new RectangularProfileValidator();

            this._calculateGeometricProperty = new CalculateGeometricProperty();

            this._piezoelectricProfileMapper = new PiezoelectricRectangularProfileMapper(
                this._arrayOperation,
                this._calculateGeometricProperty);

            this._profileMapper = new RectangularProfileMapper(
                this._arrayOperation,
                this._calculateGeometricProperty);

            this._mainMatrix = new RectangularBeamWithPiezoelectricMainMatrix(this._arrayOperation);

            this._operation = new CalculateRectangularBeamWithPiezoelectricVibration(
                this._newmarkMethod,
                this._mappingResolver,
                this._profileValidator,
                this._auxiliarOperation,
                this._profileMapper,
                this._piezoelectricProfileMapper,
                this._mainMatrix,
                this._arrayOperation);

            this._methodParameter = new NewmarkMethodParameter
            {
                InitialTime = 0,
                PeriodDivision = 100,
                NumberOfPeriods = 30,
                InitialAngularFrequency = 0.5
            };

            this._request = new BeamWithPiezoelectricRequest<RectangularProfile>
            {
                AnalysisType = "Teste",
                Author = "Teste",
                BeamData = new PiezoelectricRequestData<RectangularProfile>
                {
                    DielectricConstant = 7.33e-9,
                    DielectricPermissiveness = 30.705,
                    ElasticityConstant = 1.076e11,
                    ElectricalCharges = new List<ElectricalCharge>(),
                    ElementsWithPiezoelectric = new uint[] { 2 },
                    FirstFastening = "Pinned",
                    Forces = new List<Force> {
                        new Force
                        {
                            NodePosition = 1,
                            Value = 100
                        }
                    },
                    LastFastening = "Pinned",
                    Length = elementLength * numberOfElements,
                    Material = "Steel4130",
                    NumberOfElements = numberOfElements,
                    PiezoelectricConstant = 190e-12,
                    PiezoelectricPosition = "Up and down",
                    PiezoelectricProfile = this._piezoelectricProfile,
                    PiezoelectricSpecificMass = 7650,
                    PiezoelectricYoungModulus = 63e9,
                    Profile = this._beamProfile
                },
                MethodParameterData = this._methodParameter
            };

            this._beamWithPiezoelectic = new BeamWithPiezoelectric<RectangularProfile>()
            {
                DielectricConstant = 7.33e-9,
                DielectricPermissiveness = 30.705,
                ElasticityConstant = 1.076e11,
                ElectricalCharge = new double[piezoelectricDegreesFreedomMaximum] { 0, 0, 0 },
                ElementsWithPiezoelectric = new uint[numberOfElementsWithPiezoelectrics] { 2 },
                FirstFastening = new Pinned(),
                Forces = new double[degreesFreedomMaximum] { 0, 0, 100, 0, 0, 0 },
                GeometricProperty = new GeometricProperty
                {
                    // Beam profile: height = 3e-3, width = 25e-3.
                    Area = new double[numberOfElements] { 7.5E-05, 7.5E-05 },
                    MomentOfInertia = new double[numberOfElements] { 5.625E-11, 5.625E-11 }
                },
                LastFastening = new Pinned(),
                Length = elementLength * numberOfElements,
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                NumberOfPiezoelectricPerElements = this._numberOfPiezoelectricsPerElement,
                PiezoelectricConstant = 190e-12,
                PiezoelectricGeometricProperty = new GeometricProperty
                {
                    // Piezoelectric profile:  height = 0.267e-3, width = 25e-3.
                    Area = new double[numberOfElements] { 0, this._piezoelectricArea },
                    MomentOfInertia = new double[numberOfElements] { 0, this._piezoelectricMomentOfInertia }
                },
                PiezoelectricProfile = this._piezoelectricProfile,
                PiezoelectricSpecificMass = 7650,
                Profile = this._beamProfile,
                PiezoelectricYoungModulus = 63e9
            };

            this._equivalentForce = new double[numberOfBoundaryConditionsTrue] { 0, 100, 0, 0, 0, 0 };

            this._mass = new double[numberOfBoundaryConditionsTrue, numberOfBoundaryConditionsTrue]
            { 
                { 0.000700893, 0.0045558, -0.00052567, 0, 0, 0 },
                { 0.0045558, 0.237645, 0.00133738, -0.00534608, 0, 0 },
                { -0.00052567, 0.00133738, 0.00152337, -0.000616855, 0, 0 },
                { 0, -0.00534608, -0.000616855, 0.000822473, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0 }
            };

            this._hardness = new double[numberOfBoundaryConditionsTrue, numberOfBoundaryConditionsTrue]
            { 
                { 90, -270, 45, 0, 0, 0 },
                { -270, 2528.78, 92.1953, 362.195, 0, 0 },
                { 45, 92.1953, 210.732, 60.3659, 1.60557e-07, -1.60557e-07 },
                { 0, 362.195, 60.3659, 120.732, -1.60557e-07, 1.60557e-07 },
                { 0, 0, 1.60557e-07, -1.60557e-07, -6.8633e-07, 6.8633e-07 },
                { 0, 0, -1.60557e-07, 1.60557e-07, 6.8633e-07, -6.8633e-07 }
            };

            this._damping = new double[numberOfBoundaryConditionsTrue, numberOfBoundaryConditionsTrue]
            { 
                { 9e-05, -0.00027, 4.5e-05, 0, 0, 0 },
                { -0.00027, 0.00252878, 9.21953e-05, 0.000362195, 0, 0 },
                { 4.5e-05, 9.21953e-05, 0.000210732, 6.03659e-05, 1.60557e-13, -1.60557e-13 },
                { 0, 0.000362195, 6.03659e-05, 0.000120732, -1.60557e-13, 1.60557e-13 },
                { 0, 0, 1.60557e-13, -1.60557e-13, -6.8633e-13, 6.8633e-13 },
                { 0, 0, -1.60557e-13, 1.60557e-13, 6.8633e-13, -6.8633e-13 }
            };

            this._newmarkMethodInput = new NewmarkMethodInput
            {
                Parameter = this._methodParameter,
                NumberOfTrueBoundaryConditions = numberOfBoundaryConditionsTrue,
                Force = this._equivalentForce,
                Mass = this._mass,
                Hardness = this._hardness,
                Damping = this._damping
            };
        }

        [Fact(DisplayName = @"Feature: BuildBeam | Given: Request with dimensions of profile. | When: Invoke. | Should: Execute correctly.")]
        public async void BuildBeam_Given_DimensionsOfProfile_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.BuildBeam(this._request, degreesFreedomMaximum);

            // Assert
            result.DielectricConstant.Should().Be(this._beamWithPiezoelectic.DielectricConstant);
            result.DielectricPermissiveness.Should().Be(this._beamWithPiezoelectic.DielectricPermissiveness);
            result.ElasticityConstant.Should().Be(this._beamWithPiezoelectic.ElasticityConstant);
            result.ElectricalCharge.Should().BeEquivalentTo(this._beamWithPiezoelectic.ElectricalCharge);
            result.ElementsWithPiezoelectric.Should().BeEquivalentTo(this._beamWithPiezoelectic.ElementsWithPiezoelectric);
            result.FirstFastening.Should().BeEquivalentTo(this._beamWithPiezoelectic.FirstFastening);
            result.Forces.Should().BeEquivalentTo(this._beamWithPiezoelectic.Forces);
            for (int i = 0; i < numberOfElements; i++)
            {
                result.GeometricProperty.Area[i].Should().BeApproximately(this._beamWithPiezoelectic.GeometricProperty.Area[i], this._precision);
                result.GeometricProperty.MomentOfInertia[i].Should().BeApproximately(this._beamWithPiezoelectic.GeometricProperty.MomentOfInertia[i], this._precision);
                result.PiezoelectricGeometricProperty.Area[i].Should().BeApproximately(this._beamWithPiezoelectic.PiezoelectricGeometricProperty.Area[i], this._precision);
                result.PiezoelectricGeometricProperty.MomentOfInertia[i].Should().BeApproximately(this._beamWithPiezoelectic.PiezoelectricGeometricProperty.MomentOfInertia[i], this._precision);
            }
            result.LastFastening.Should().BeEquivalentTo(this._beamWithPiezoelectic.LastFastening);
            result.Length.Should().Be(this._beamWithPiezoelectic.Length);
            result.Material.Should().BeEquivalentTo(this._beamWithPiezoelectic.Material);
            result.NumberOfElements.Should().Be(this._beamWithPiezoelectic.NumberOfElements);
            result.NumberOfPiezoelectricPerElements.Should().Be(this._beamWithPiezoelectic.NumberOfPiezoelectricPerElements);
            result.PiezoelectricConstant.Should().Be(this._beamWithPiezoelectic.PiezoelectricConstant);
            result.PiezoelectricProfile.Should().BeEquivalentTo(this._beamWithPiezoelectic.PiezoelectricProfile);
            result.PiezoelectricSpecificMass.Should().Be(this._beamWithPiezoelectic.PiezoelectricSpecificMass);
            result.PiezoelectricYoungModulus.Should().Be(this._beamWithPiezoelectic.PiezoelectricYoungModulus);
            result.Profile.Should().BeEquivalentTo(this._beamWithPiezoelectic.Profile);
        }

        [Fact(DisplayName = @"Feature: CreateInput | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CreateInput_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CreateInput(this._beamWithPiezoelectic, this._methodParameter, degreesFreedomMaximum);

            // Assert
            result.Parameter.Should().BeEquivalentTo(this._methodParameter);
            result.NumberOfTrueBoundaryConditions.Should().Be(numberOfBoundaryConditionsTrue);
            result.Force.Should().BeEquivalentTo(this._equivalentForce);
            for(int i = 0; i < numberOfBoundaryConditionsTrue; i++)
            {
                for (int j = 0; j < numberOfBoundaryConditionsTrue; j++)
                {
                    result.Mass[i, j].Should().BeApproximately(this._mass[i,j], precision: 5e-7);
                    result.Hardness[i, j].Should().BeApproximately(this._hardness[i,j], precision: 5e-3);
                    result.Damping[i, j].Should().BeApproximately(this._damping[i,j], precision: 5e-9);
                }
            }
        }
    }
}
