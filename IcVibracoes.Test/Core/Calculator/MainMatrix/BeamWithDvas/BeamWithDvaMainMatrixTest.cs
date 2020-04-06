using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Models.Characteristics;
using Moq;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.BeamWithDvas
{
    public class BeamWithDvaMainMatrixTest<T, TProfile>
        where T : BeamWithDvaMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        private const int DegreesFreedomMaximum = 6;
        private const int NumberOfDvas = 1;

        protected double[,] MassMatrix { get; set; }
        protected double[,] HardnessMatrix { get; set; }

        protected double[,] MassWithDvaMatrix { get; set; }
        protected double[,] HardnessWithDvaMatrix { get; set; }


        private readonly Mock<T> _operationMock;

        private readonly bool[] _boundaryConditions;

        private readonly double[] _dvaMasses;
        private readonly double[] _dvaHardnesses;
        private readonly uint[] _dvaNodePositions;

        private readonly double _precision;

        public BeamWithDvaMainMatrixTest()
        {
            this._operationMock = new Mock<T>();

            this._dvaMasses = new double[NumberOfDvas] { 0.012 };
            this._dvaHardnesses = new double[NumberOfDvas] { 201869.25 };
            this._dvaNodePositions = new uint[NumberOfDvas] { 1 };

            // Bondary conditions for a beam that is pinned in both fastenings.
            this._boundaryConditions = new bool[DegreesFreedomMaximum + NumberOfDvas] { true, false, true, true, true, false, true };

            this._precision = 1e-18;
        }

        [Fact(DisplayName = @"Feature: CalculateMassWithDva | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMassWithDva_Should_ExecuteCorrectly()
        {
            // Arrange
            int size = DegreesFreedomMaximum + NumberOfDvas;

            // Act
            var result = await this._operationMock.Object.CalculateMassWithDva(MassMatrix, this._dvaMasses, this._dvaNodePositions);

            // Assert
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j].Should().BeApproximately(MassWithDvaMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateHardnessWithDva | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateHardnessWithDva_Should_ExecuteCorrectly()
        {
            // Arrange
            int size = DegreesFreedomMaximum + NumberOfDvas;

            // Act
            var result = await this._operationMock.Object.CalculateHardnessWithDva(HardnessMatrix, this._dvaHardnesses, this._dvaNodePositions);

            // Assert
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j].Should().BeApproximately(HardnessWithDvaMatrix[i, j], this._precision);
                }
            }
        }

        [Fact(DisplayName = @"Feature: CalculateBondaryCondition | Given: Beam pinned in both fastening. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateBondaryCondition_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operationMock.Object.CalculateBondaryCondition(new Pinned(), new Pinned(), DegreesFreedomMaximum, NumberOfDvas);

            // Assert
            result.Should().BeEquivalentTo(this._boundaryConditions);
        }
    }
}
