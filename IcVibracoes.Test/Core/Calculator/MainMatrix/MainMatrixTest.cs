using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix
{
    public abstract class MainMatrixTest<TBeam, TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        private readonly Mock<MainMatrix<TBeam, TProfile>> _operationMock;
        private readonly double[,] _elementMass;
        private readonly double[,] _elementStiffness;
        private readonly double[,] _mass;
        private readonly double[,] _stiffness;
        private readonly double[,] _damping;
        private readonly double[] _force;
        private readonly bool[] _boundaryConditions;
        private readonly uint _numberOfTrueBoundaryConditions;
        private readonly uint _degressOfFreedom;
        private readonly TBeam _beam;

        private const double _forceValue = 10;

        public MainMatrixTest()
        {
            this._operationMock = new Mock<MainMatrix<TBeam, TProfile>>();

            uint numberOfElements = 2;
            uint numberOfNodes = numberOfElements + 1;
            this._degressOfFreedom = numberOfNodes * Constants.DegreesOfFreedomPerNode;

            this._beam = new TBeam
            {
                Fastenings = new Dictionary<uint, FasteningType> { { 0, FasteningType.Pinned }, { numberOfNodes, FasteningType.Pinned } },
                Forces = new double[] { 0, 0, _forceValue, 0, 0, 0 },
                GeometricProperty = GeometricProperty.Empty,
                Length = 1,
                Material = Material.Steel1020,
                NumberOfElements = numberOfElements,
                //Profile = new TProfile
                //{
                //    Area = ,
                //    MomentOfInertia =
                //}
            };

            this._elementMass = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];

            this._elementStiffness = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];

            this._mass = new double[this._degressOfFreedom, this._degressOfFreedom];

            this._stiffness = new double[this._degressOfFreedom, this._degressOfFreedom];

            this._damping = new double[this._degressOfFreedom, this._degressOfFreedom];

            this._force = new double[this._degressOfFreedom];

            this._boundaryConditions = new bool[] { true, false, true, true, true, false };

            this._numberOfTrueBoundaryConditions = 4;
        }

        //[Fact(DisplayName = @"Feature: CalculateElementMass | When: Execute. | Given: Valid parameters. | Should: Return the element's mass matrix.")]
        //public async Task CalculateElementMass_Should_ReturnElementMassMatrix()
        //{
        //    // Act
        //    var result = await this._operationMock.Object.CalculateElementMass(this._beam, this._degressOfFreedom).ConfigureAwait(false);

        //    // Assert
        //    for (int i = 0; i < result.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < result.GetLength(0); j++)
        //        {
        //            result[i, j].Should().BeApproximately(this._elementMass[i, j], precision: 1e-3);
        //        }
        //    }
        //}

        [Fact(DisplayName = @"Feature: CalculateMass | When: Execute. | Given: Valid parameters. | Should: Return the element's mass matrix.")]
        public void CalculateMass_Should_ReturnElementMassMatrix()
        {
            // Act
            var result = this._operationMock.Object.CalculateMass(this._beam, this._degressOfFreedom);

            // Assert
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(0); j++)
                {
                    result[i, j].Should().BeApproximately(this._elementMass[i, j], precision: 1e-3);
                }
            }
        }
    }
}
