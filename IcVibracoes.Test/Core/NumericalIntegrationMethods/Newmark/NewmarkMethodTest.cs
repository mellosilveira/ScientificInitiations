using FluentAssertions;
using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Methods.AuxiliarOperations;
using Moq;
using Xunit;

namespace IcVibracoes.Test.Core.NumericalIntegrationMethods.Newmark
{
    public class NewmarkMethodTest
    {
        private const int piezoelectricDegreesFreedomMaximum = 3;
        private const int degreesFreedomMaximum = 6;
        private const int numberOfrueBoundaryConditions = 6;

        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly INewmarkMethodValidator _newmarkMethodValidator;
        private readonly IArrayOperation _arrayOperation;
        private readonly NewmarkMethod _operation;

        private readonly NewmarkMethodParameter _methodParameter;
        private readonly NewmarkMethodInput _newmarkMethodInput;
        private readonly double _precision;
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
                { 90, -270, 45, 0, 0, 0 },
                { -270, 2528.78, 92.1953, 362.195, 0, 0 },
                { 45, 92.1953, 210.732, 60.3659, 1.60557e-07, -1.60557e-07 },
                { 0, 362.195, 60.3659, 120.732, -1.60557e-07, 1.60557e-07 },
                { 0, 0, 1.60557e-07, -1.60557e-07, -6.8633e-07, 6.8633e-07 },
                { 0, 0, -1.60557e-07, 1.60557e-07, 6.8633e-07, -6.8633e-07 }
            };

            this._equivalentForce = new double[numberOfrueBoundaryConditions] { 0, 100, 0, 0, 0, 0, };
        }

        [Fact(DisplayName = @"Feature: CalculateEquivalentHardness | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateEquivalentHardness_Should_ExecuteCorrectly()
        {
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
