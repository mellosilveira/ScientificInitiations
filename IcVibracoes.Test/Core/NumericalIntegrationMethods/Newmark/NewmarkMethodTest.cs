using FluentAssertions;
using IcVibracoes.Common.Classes;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark;
using System;
using Xunit;

namespace IcVibracoes.Test.Core.NumericalIntegrationMethods.Newmark
{
    public class NewmarkMethodTest
    {
        private const int piezoelectricDegreesFreedomMaximum = 3;
        private const int degreesFreedomMaximum = 6;
        private const int numberOfrueBoundaryConditions = 6;

        private double _precision;
        
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly INewmarkMethodValidator _newmarkMethodValidator;
        private readonly IArrayOperation _arrayOperation;
        private readonly NewmarkMethod _operation;

        private readonly NewmarkMethodParameter _methodParameter;
        private readonly NewmarkMethodInput _newmarkMethodInput;
        private readonly double[] _previousDisplacement;
        private readonly double[] _previousVelocity;
        private readonly double[] _previousAcceleration;
        private readonly double[] _displacement;
        private readonly double[] _velocity;
        private readonly double[] _acceleration;
        private readonly double[] _force;
        private readonly double[] _equivalentForce;
        private readonly double[,] _mass;
        private readonly double[,] _hardness;
        private readonly double[,] _damping;
        private readonly double[,] _equivalentHardness;

        public NewmarkMethodTest()
        {
            this._precision = 1e-15;

            this._arrayOperation = new ArrayOperation();
            this._auxiliarOperation = new AuxiliarOperation();
            this._newmarkMethodValidator = new NewmarkMethodValidator();

            this._operation = new NewmarkMethod(
                this._arrayOperation,
                this._auxiliarOperation,
                this._newmarkMethodValidator);

            this._methodParameter = new NewmarkMethodParameter
            {
                InitialTime = 0,
                PeriodDivision = 100,
                NumberOfPeriods = 30,
                InitialAngularFrequency = 0.5
            };

            this._force = new double[numberOfrueBoundaryConditions] { 0, 100, 0, 0, 0, 0 };

            this._mass = new double[numberOfrueBoundaryConditions, numberOfrueBoundaryConditions]
            {
                { 0.000700893, 0.0045558, -0.00052567, 0, 0, 0 },
                { 0.0045558, 0.237645, 0.00133738, -0.00534608, 0, 0 },
                { -0.00052567, 0.00133738, 0.00152337, -0.000616855, 0, 0 },
                { 0, -0.00534608, -0.000616855, 0.000822473, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0 }
            };

            this._hardness = new double[numberOfrueBoundaryConditions, numberOfrueBoundaryConditions]
            {
                { 90, -270, 45, 0, 0, 0 },
                { -270, 2528.78, 92.1953, 362.195, 0, 0 },
                { 45, 92.1953, 210.732, 60.3659, 1.60557e-07, -1.60557e-07 },
                { 0, 362.195, 60.3659, 120.732, -1.60557e-07, 1.60557e-07 },
                { 0, 0, 1.60557e-07, -1.60557e-07, -6.8633e-07, 6.8633e-07 },
                { 0, 0, -1.60557e-07, 1.60557e-07, 6.8633e-07, -6.8633e-07 }
            };

            this._damping = new double[numberOfrueBoundaryConditions, numberOfrueBoundaryConditions]
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
                AngularFrequency = this._methodParameter.InitialAngularFrequency,
                NumberOfTrueBoundaryConditions = numberOfrueBoundaryConditions,
                Force = this._force,
                Mass = this._mass,
                Hardness = this._hardness,
                Damping = this._damping
            };

            this._previousDisplacement = new double[numberOfrueBoundaryConditions];
            this._previousVelocity = new double[numberOfrueBoundaryConditions];
            this._previousAcceleration = new double[numberOfrueBoundaryConditions];

            this._displacement = new double[numberOfrueBoundaryConditions];
            this._velocity = new double[numberOfrueBoundaryConditions];
            this._acceleration = new double[numberOfrueBoundaryConditions];

            this._equivalentHardness = new double[numberOfrueBoundaryConditions, numberOfrueBoundaryConditions]
            { 
                { 90.179, -268.85, 44.8676, 0, 0, 0 },
                { -268.85, 2589.02, 92.5356, 360.847, 0, 0 },
                { 44.8676, 92.5356, 211.121, 60.2106, 1.6056e-07, -1.6056e-07 },
                { 0, 360.847, 60.2106, 120.942, -1.6056e-07, 1.6056e-07 },
                { 0, 0, 1.6056e-07, -1.6056e-07, -6.86341e-07, 6.86341e-07 },
                { 0, 0, -1.6056e-07, 1.6056e-07, 6.86341e-07, -6.86341e-07 }
            };

            this._equivalentForce = new double[numberOfrueBoundaryConditions] { 0, 100, 0, 0, 0, 0, };
        }

        [Fact(DisplayName = @"Feature: CalculateIngrationContants | Given: Valid delta time. | When: Invoke. | Should: Execute correctly.")]
        public void CalculateIngrationContants_Should_ExecuteCorrectly()
        {
            // Arrange
            double expected_a0 = 253.303;
            double expected_a1 = 15.9155;
            double expected_a2 = 31.831;
            double expected_a3 = 1;
            double expected_a4 = 1;
            double expected_a5 = 0;
            double expected_a6 = 0.0628319;
            double expected_a7 = 0.0628319;

            double deltaTime = (Math.PI * 2 / this._newmarkMethodInput.AngularFrequency) / this._newmarkMethodInput.Parameter.PeriodDivision;

            // Act
            this._operation.CalculateIngrationContants(deltaTime);

            // Assert
            this._operation.a0.Should().BeApproximately(expected_a0, precision: 5e-4);
            this._operation.a1.Should().BeApproximately(expected_a1, precision: 5e-5);
            this._operation.a2.Should().BeApproximately(expected_a2, precision: 5e-4);
            this._operation.a3.Should().Be(expected_a3);
            this._operation.a4.Should().Be(expected_a4);
            this._operation.a5.Should().Be(expected_a5);
            this._operation.a6.Should().BeApproximately(expected_a6, precision: 5e-8);
            this._operation.a7.Should().BeApproximately(expected_a7, precision: 5e-8);
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentHardness_Should_ExecuteCorrectly()
        {
            // Arrange
            double deltaTime = (Math.PI * 2 / this._newmarkMethodInput.AngularFrequency) / this._newmarkMethodInput.Parameter.PeriodDivision;
            this._operation.CalculateIngrationContants(deltaTime);

            this._precision = 5e-3;

            // Act
            var result = await this._operation.CalculateEquivalentHardness(this._mass, this._hardness, this._damping, numberOfrueBoundaryConditions);

            // Assert
            for (int i = 0; i < numberOfrueBoundaryConditions; i++)
            {
                for (int j = 0; j < numberOfrueBoundaryConditions; j++)
                {
                    result[i, j].Should().BeApproximately(this._equivalentHardness[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentForce | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentForce_Should_ExecuteCorrectly()
        {
            // Arrange
            double deltaTime = (Math.PI * 2 / this._newmarkMethodInput.AngularFrequency) / this._newmarkMethodInput.Parameter.PeriodDivision;
            this._operation.CalculateIngrationContants(deltaTime);

            // Act
            var result = await this._operation.CalculateEquivalentForce(this._newmarkMethodInput, this._previousDisplacement, this._previousVelocity, this._previousAcceleration);

            // Assert
            for (int i = 0; i < numberOfrueBoundaryConditions; i++)
            {
                result[i].Should().BeApproximately(this._equivalentForce[i], this._precision);
            }
        }
    }
}
