using FluentAssertions;
using IcVibracoes.Core.Calculator.ArrayOperations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator
{
    public class ArrayOperationTest
    {
        private const int arraySize = 4;
        private readonly ArrayOperation _operation;
        private readonly double[,] _matrix1;
        private readonly double[] _vector1;
        private readonly double[] _vector2;
        private readonly double[] _vector3;
        private readonly double[] _valuesToAdd;
        private readonly uint[] _nodePositions;
        private readonly double[] _sumVector1Vector2Vector3;
        private readonly double[,] _inversedMatrix1;
        private readonly double[,] _transposedMatrix1;
        private readonly double[,] _addedValuesInMatrix1;
        private readonly double[] _mergedVector1Vetor2;
        private readonly double[] _multipliedMatrix1Vector1;
        private readonly double[,] _equivalentHardness;

        private uint[] _elementPositions;

        public ArrayOperationTest()
        {
            this._operation = new ArrayOperation();
            this._matrix1 = new double[arraySize, arraySize] { { 1, 1, 1, 1 }, { 2, 5, 1, 4 }, { 3, 5, 2, 0 }, { 2, 5, 0, 1 } };
            this._vector1 = new double[arraySize] { 1, 2, 3, 4 };
            this._vector2 = new double[arraySize] { 5, 0, 1, 3 };
            this._vector3 = new double[arraySize] { 5, 6, 7, 8 };
            this._valuesToAdd = new double[2] { -1, 1 };
            this._nodePositions = new uint[2] { 0, 1 };
            this._elementPositions = new uint[2] { 1, 2 };
            this._sumVector1Vector2Vector3 = new double[arraySize] { 11, 8, 11, 15 };
            this._addedValuesInMatrix1 = new double[arraySize, arraySize] { { 0, 1, 1, 1 }, { 2, 5, 1, 4 }, { 3, 5, 3, 0 }, { 2, 5, 0, 1 } };
            this._inversedMatrix1 = new double[arraySize, arraySize] { { 3.5, -1.3, -1.1, 1.7 }, { -1.5, 0.5, 0.5, -0.5 }, { -1.5, 0.7, 0.9, -1.3 }, { 0.5, 0.1, -0.3, 0.1 } };
            this._mergedVector1Vetor2 = new double[2 * arraySize] { 1, 2, 3, 4, 5, 0, 1, 3 };
            this._multipliedMatrix1Vector1 = new double[arraySize] { 10, 31, 19, 16 };
            this._transposedMatrix1 = new double[arraySize, arraySize] { { 1, 2, 3, 2 }, { 1, 5, 5, 5 }, { 1, 1, 2, 0 }, { 1, 4, 0, 1 } };

            this._equivalentHardness = new double[,]
            {
                { 90, -270, 45, 0, 0, 0 },
                { -270, 2528.78, 92.1953, 362.195, 0, 0 },
                { 45, 92.1953, 210.732, 60.3659, 1.60557e-07, -1.60557e-07 },
                { 0, 362.195, 60.3659, 120.732, -1.60557e-07, 1.60557e-07 },
                { 0, 0, 1.60557e-07, -1.60557e-07, -6.8633e-07, 6.8633e-07 },
                { 0, 0, -1.60557e-07, 1.60557e-07, 6.8633e-07, -6.8633e-07 }
            };
        }

        [Fact(DisplayName = @"Feature: AddValue | Given: Valid matrix. | When: Invoke. | Should: Return correctly matrix size and values.")]
        public async void AddValue_ValidMatrix_ShouldExecuteCorrectly()
        {
            // Arrange
            double[,] matrixToAdd = this._matrix1;

            // Act
            double[,] result = await this._operation.AddValue(matrixToAdd, this._valuesToAdd, this._nodePositions, "teste");

            // Assert
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(this._addedValuesInMatrix1);
        }

        [Fact(DisplayName = @"Feature: AddValue | Given: Value and node position matrix with different lengths. | When: Invoke. | Should: Throw ArgumentException.")]
        public void AddValue_InvalidNumberOfValuesAndPositions_ShouldThrowArgumentException()
        {
            // Arrange
            double[] values = new double[2] { 0, 1 };
            uint[] nodePositions = new uint[3] { 0, 1, 2 };

            // Act
            Func<Task<double[,]>> act = async () => await this._operation.AddValue(this._matrix1, values, nodePositions, "teste");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = @"Feature: AddValue | Given: Invalid node positions. | When: Invoke. | Should: Throw ArgumentException.")]
        public void AddValue_InvalidNodePositions_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            uint[] nodePositions = new uint[2] { 5, 6 };

            // Act
            Func<Task<double[,]>> act = async () => await this._operation.AddValue(this._matrix1, this._valuesToAdd, nodePositions, "teste");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        //268435456 --> maximum size of double array --> maximum size of array = 2Gb, size of double = 8bytes
        [Fact(DisplayName = @"Feature: CreateVector | Given: Valid sizes. | When: Create a full vector. | Should: Return correctly array size and values.")]
        public async void CreateVector_When_CreateFullVector_ShouldExecuteCorrectly()
        {
            // Arrange
            Random random = new Random();
            double value = random.NextDouble();

            // Act
            double[] result = await this._operation.CreateVector(value, arraySize, "test");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(arraySize);
            result.Should().OnlyContain(eoq => eoq == value);
        }

        [Fact(DisplayName = @"Feature: CreateVector | Given: Invalid size. | When: Create a full vector. | Should: Throw argument expection.")]
        public void CreateVector_When_CreateFullVector_Should_ThrowArgumentException()
        {
            // Arrange / Act
            Func<Task<double[]>> act = async () =>
            {
                Random random = new Random();
                double value = random.NextDouble();

                return await this._operation.CreateVector(value, size: 0, "teste");
            };

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = @"Feature: CreateVector | Given: Invalid matrix size. | When: Create a vector based in the node positions. | Should: Throw ArgumentException.")]
        public void CreateVector_Should_ThrowArgumentException()
        {
            // Arrange / Act
            Func<Task<double[]>> act = async () =>
            {
                Random random = new Random();
                double value = random.NextDouble();

                return await this._operation.CreateVector(value, size: 0, this._elementPositions, "teste");
            };

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = @"Feature: CreateVector | Given: Invalid positions. | When: Invoke. | Should: Throw ArgumentOutOfRangeException.")]
        public void CreateVector_Should_ThrowArgumentOutOfRangeException()
        {
            // Arrange
            this._elementPositions = new uint[2] { 5, 6 };
            Random random = new Random();
            double value = random.NextDouble();

            // Act
            Func<Task<double[]>> act = async () => await this._operation.CreateVector(value, arraySize, this._elementPositions, "teste");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = @"Feature: CreateVector | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CreateVector_Should_ExecuteCorrectly()
        {
            // Arrange
            Random random = new Random();
            double value = random.NextDouble();
            double[] expected = new double[arraySize] { value, value, 0, 0 };

            // Act
            var result = await this._operation.CreateVector(value, arraySize, this._elementPositions, "teste");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(arraySize);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact(DisplayName = @"Feature: InverseMatrix | Given: Valid matrix. | When: Inverse matrix. | Should: Return correctly matrix size and values.")]
        public async void InverseMatrix_ValidMatrix_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.InverseMatrix(this._matrix1, "Inverse Test");

            // Assert
            result.Should().NotBeEmpty();
            result.GetLength(0).Should().Be(this._matrix1.GetLength(0));
            result.GetLength(1).Should().Be(this._matrix1.GetLength(1));

            Parallel.For(0, arraySize, i =>
            {
                Parallel.For(0, arraySize, j =>
                {
                    result[i, j].Should().BeApproximately(this._inversedMatrix1[i, j], precision: 1e-15);
                });
            });
        }

        [Fact(DisplayName = @"Feature: InverdeMatrix | Given: Invalid matrix. | When: Invoke. | Should: Throw ArgumentException.")]
        public void InverseMatrix_Should_ThrowArgumentException()
        {
            // Arrange
            double[,] matrix = new double[2, 3];

            // Act
            Func<Task<double[,]>> act = async () => await this._operation.InverseMatrix(matrix, "teste");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = @"Feature: MergeVectors | Given: Valid vectors. | When: Invoke. | Should: Execute correctly.")]
        public async void MergeVectors_Shoudl_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.MergeVectors(this._vector1, this._vector2);

            // Assert
            result.Should().BeEquivalentTo(this._mergedVector1Vetor2);
        }

        [Fact(DisplayName = @"Feature: Multiply | Given: One matrix and one array. | When: Multiply matrix and array. | Should: Return correctly array size and values.")]
        public async void Multiply_MatrixAndArray_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Multiply(this._matrix1, this._vector1, "Multiply Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._vector1.Length);
            result.Should().BeEquivalentTo(this._multipliedMatrix1Vector1);
        }

        [Fact(DisplayName = @"Feature: Sum | Given: Valid vectors. | When: Invoke. | Should: Exeute correctly.")]
        public async void Sum_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.Sum(this._vector1, this._vector2, this._vector3, "teste");

            // ASsert
            result.Should().BeEquivalentTo(this._sumVector1Vector2Vector3);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: One matrix. | When: Transpose matrix. | Should: Return correctly matrix size and values.")]
        public async void TransposeMatrix_ValidMatrix_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.TransposeMatrix(this._matrix1);

            // Assert
            result.Should().NotBeEmpty();
            result.GetLength(0).Should().Be(this._matrix1.GetLength(1));
            result.GetLength(1).Should().Be(this._matrix1.GetLength(0));
            result.Should().BeEquivalentTo(this._transposedMatrix1);
        }
    }
}
