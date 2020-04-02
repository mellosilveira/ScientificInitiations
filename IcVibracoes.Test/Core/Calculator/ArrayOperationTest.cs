using FluentAssertions;
using IcVibracoes.Core.Calculator.ArrayOperations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator
{
    public class ArrayOperationTest
    {
        private readonly ArrayOperation _operation;
        private readonly double[,] _matrix1;
        private readonly double[,] _matrix2;
        private readonly double[] _array1;
        private readonly double[] _array2;
        private readonly double[] _valuesToAdd;
        private readonly uint[] _nodePositions;
        private readonly double[,] _inversedMatrix1;
        private readonly double[,] _multipliedMatrix1Matrix2;
        private readonly double[] _multipliedArray1Matrix1;
        private readonly double[] _multipliedMatrix1Array1;
        private readonly double[,] _subtractedMatrix1Matrix2;
        private readonly double[] _subtractedArray1Array2;
        private readonly double[,] _addedMatrix1Matrix2;
        private readonly double[] _addedArray1Array2;
        private readonly double[,] _transposedMatrix1;
        private readonly double[,] _addedValuesInMatrix1;

        private uint[] _elementPositions;
        
        public ArrayOperationTest()
        {
            this._operation = new ArrayOperation();
            this._matrix1 = new double[4, 4] { { 1, 1, 1, 1 }, { 2, 5, 1, 4 }, { 3, 5, 2, 0 }, { 2, 5, 0, 1 } };
            this._matrix2 = new double[4, 4] { { 2, 1, 3, 1 }, { 4, 1, 1, 4 }, { 3, 1, 2, 1 }, { 0, 1, 1, 2 } };
            this._array1 = new double[4] { 1, 2, 3, 4 };
            this._array2 = new double[4] { 5, 0, 1, 3 };
            this._valuesToAdd = new double[2] { -1, 1 };
            this._nodePositions = new uint[2] { 0, 1 };
            this._elementPositions = new uint[2] { 1, 2 };
            this._addedValuesInMatrix1 = new double[4, 4] { { 0, 1, 1, 1 }, { 2, 5, 1, 4 }, { 3, 5, 3, 0 }, { 2, 5, 0, 1 } };
            this._inversedMatrix1 = new double[4, 4] { { 3.5, -1.3, -1.1, 1.7 }, { -1.5, 0.5, 0.5, -0.5 }, { -1.5, 0.7, 0.9, -1.3 }, { 0.5, 0.1, -0.3, 0.1 } };
            this._multipliedMatrix1Matrix2 = new double[4, 4] { { 9, 4, 7, 8 }, { 27, 12, 17, 31 }, { 32, 10, 18, 25 }, { 24, 8, 12, 24 } };
            this._multipliedArray1Matrix1 = new double[4] { 22, 46, 9, 13 };
            this._multipliedMatrix1Array1 = new double[4] { 10, 31, 19, 16 };
            this._subtractedMatrix1Matrix2 = new double[4, 4] { { -1, 0, -2, 0 }, { -2, 4, 0, 0 }, { 0, 4, 0, -1 }, { 2, 4, -1, -1 } };
            this._subtractedArray1Array2 = new double[4] { 6, 2, 4, 7 };
            this._addedMatrix1Matrix2 = new double[4, 4] { { 3, 2, 4, 2 }, { 6, 6, 2, 8 }, { 6, 6, 4, 1 }, { 2, 6, 1, 3 } };
            this._addedArray1Array2 = new double[4] { -4, 2, 2, 1 };
            this._transposedMatrix1 = new double[4, 4] { { 1, 2, 3, 2 }, { 1, 5, 5, 5 }, { 1, 1, 2, 0 }, { 1, 4, 0, 1 } };
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
            uint size = 4;
            Random random = new Random();
            double value = random.NextDouble();

            // Act
            double[] result = await this._operation.CreateVector(value, size, "test");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount((int)size);
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
            Func<Task<double[]>> act = async () => await this._operation.CreateVector(value, size: 4, this._elementPositions, "teste");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = @"Feature: CreateVector | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public async void CreateVector_Should_ExecuteCorrectly()
        {
            // Arrange
            Random random = new Random();
            double value = random.NextDouble();
            uint size = 4;
            double[] expected = new double[4] { value, value, 0, 0 };

            // Act
            var result = await this._operation.CreateVector(value, size, this._elementPositions, "teste");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount((int)size);
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

            Parallel.For(0, 4, i =>
            {
                Parallel.For(0, 4, j =>
                {
                    result[i, j].Should().BeApproximately(this._inversedMatrix1[i, j], 2);
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

        [Fact(DisplayName = @"Feature: Multiply | Given: One matrix and one array. | When: Multiply matrix and array. | Should: Return correctly array size and values.")]
        public async void Multiply_MatrixAndArray_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Multiply(this._matrix1, this._array1, "Multiply Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._matrix1.Length);
            result.Should().BeEquivalentTo(this._multipliedMatrix1Array1);
        }

        [Fact(DisplayName = @"Feature: Multiply | Given: One array and one matrix. | When: Multiply array and matrix. | Should: Return correctly array size and values.")]
        public async void Multiply_ArrayAndMatrix_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Multiply(this._array1, this._matrix1, "Multiply test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._matrix1.Length);
            result.Should().BeEquivalentTo(this._multipliedArray1Matrix1);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Two matrixes. | When: Subtract matrixes. | Should: Return correctly matrix size and values.")]
        public async void Subtract_TwoMatrixes_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.Subtract(this._matrix1, this._matrix2, "Subtract Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._matrix1.Length);
            result.Length.Should().Be(this._matrix2.Length);
            result.Should().BeEquivalentTo(this._subtractedMatrix1Matrix2);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Two arrays. | When: Subtract arrays. | Should: Return correctly array size and values.")]
        public async void Subtract_TwoArrays_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Subtract(this._array1, this._array2, "Subtract Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._array1.Length);
            result.Length.Should().Be(this._array1.Length);
            result.Should().BeEquivalentTo(this._subtractedArray1Array2);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Two matrixes. | When: Sum matrixes. | Should: Return correctly matrix size and values.")]
        public async void Sum_TwoMatrixes_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.Sum(this._matrix1, this._matrix2, "Sum Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._matrix1.Length);
            result.Length.Should().Be(this._matrix2.Length);
            result.Should().BeEquivalentTo(this._addedMatrix1Matrix2);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Two arrays. | When: Sum arrays. | Should: Return correctly array size and values.")]
        public async void Sum_TwoArrays_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Sum(this._array1, this._array2, "Sum test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._array1.Length);
            result.Length.Should().Be(this._array1.Length);
            result.Should().BeEquivalentTo(this._addedArray1Array2);
        }


        [Fact(DisplayName = @"Feature: ArrayOperation | Given: One matrix. | When: Transpose matrix. | Should: Return correctly matrix size and values.")]
        public async void TransposeMatrix_ValidMatrix_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.InverseMatrix(this._matrix1, "Inverse Test");

            // Assert
            result.Should().NotBeEmpty();
            result.GetLength(0).Should().Be(this._matrix1.GetLength(1));
            result.GetLength(1).Should().Be(this._matrix1.GetLength(0));
            result.Should().BeEquivalentTo(this._transposedMatrix1);
        }
    }
}
