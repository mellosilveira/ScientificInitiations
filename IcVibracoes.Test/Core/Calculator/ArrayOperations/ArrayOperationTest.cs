using FluentAssertions;
using IcVibracoes.Core.Calculator.ArrayOperations;
using System;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator.ArrayOperations
{
    public class ArrayOperationTest
    {
        private readonly ArrayOperation _operation;
        private readonly double[,] _matrix1;
        private readonly double[,] _matrix2;
        private readonly double[] _array1;
        private readonly double[] _array2;
        private readonly double[,] _inversedMatrix1;
        private readonly double[,] _multipliedMatrix1Matrix2;
        private readonly double[,] _multipliedArray1Matrix1;
        private readonly double[,] _multipliedMatrix1Array1;
        private readonly double[,] _subtractedMatrix1Matrix2;
        private readonly double[] _subtractedArray1Array2;
        private readonly double[,] _addedMatrix1Matrix2;
        private readonly double[] _addedArray1Array2;
        private readonly double[,] _transposedMatrix1;

        public ArrayOperationTest()
        {
            this._operation = new ArrayOperation();
            this._matrix1 = new double[4, 4] { { 1, 1, 1, 1 }, { 2, 5, 1, 4 }, { 3, 5, 2, 0 }, { 2, 5, 1, 0 } };
            this._matrix2 = new double[4, 4] { { 2, 1, 3, 1 }, { 4, 1, 1, 4 }, { 3, 1, 2, 1 }, { 0, 1, 1, 2 } };
            this._array1 = new double[4] { 1, 2, 3, 4 };
            this._array2 = new double[4] { 5, 0, 1, 3 };
            this._inversedMatrix1 = new double[4, 4];
            this._multipliedMatrix1Matrix2 = new double[4, 4];
            this._multipliedArray1Matrix1 = new double[4, 4];
            this._multipliedMatrix1Array1 = new double[4, 4];
            this._subtractedMatrix1Matrix2 = new double[4, 4];
            this._subtractedArray1Array2 = new double[4];
            this._addedMatrix1Matrix2 = new double[4, 4]; 
            this._addedArray1Array2 = new double[4];
            this._transposedMatrix1 = new double[4, 4];
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Valid matrix. | When: Inverse matrix. | Should: Return correctly matrix size and values.")]
        public async void InverseMatrix_ValidMatrix_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.InverseMatrix(this._matrix1, "Inverse Test");

            // Assert
            result.Should().NotBeEmpty();
            result.GetLength(0).Should().Be(this._matrix1.GetLength(0));
            result.GetLength(1).Should().Be(this._matrix1.GetLength(1));
            result.Should().BeEquivalentTo(this._inversedMatrix1);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: Two matrixes. | When: Multiply matrixes. | Should: Return correctly matrix size and values.")]
        public async void Multiply_TwoMatrixes_ShouldExecuteCorrectly()
        {
            // Act
            double[,] result = await this._operation.Multiply(this._matrix1, this._matrix2, "Multiply Test");

            // Assert
            result.Should().NotBeEmpty();
            result.GetLength(0).Should().Be(this._matrix1.GetLength(0));
            result.GetLength(1).Should().Be(this._matrix2.GetLength(1));
            result.Should().BeEquivalentTo(this._multipliedMatrix1Matrix2);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: One matrix and one array. | When: Multiply matrix and array. | Should: Return correctly array size and values.")]
        public async void Multiply_MatrixAndArray_ShouldExecuteCorrectly()
        {
            // Act
            double[] result = await this._operation.Multiply(this._matrix1, this._array1,"Multiply Test");

            // Assert
            result.Should().NotBeEmpty();
            result.Length.Should().Be(this._matrix1.Length);
            result.Should().BeEquivalentTo(this._multipliedMatrix1Array1);
        }

        [Fact(DisplayName = @"Feature: ArrayOperation | Given: One array and one matrix. | When: Multiply array and matrix. | Should: Return correctly array size and values.")]
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

        [Theory(DisplayName = @"Feature: ArrayOperation | Given: Valid sizes. | When: Create array. | Should: Return correctly array size and values.")]
        [InlineData(uint.MinValue)]
        [InlineData(1)]
        [InlineData(1000)]
        //268435456 --> maximum size of double array --> maximum size of array = 2Gb, size of double = 8bytes
        public async void Create_ShouldExecuteCorrectly(uint size)
        {
            // Arrange
            Random random = new Random();
            double value = random.NextDouble();

            // Act
            double[] result = await this._operation.Create(value, size);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount((int)size);
            foreach (double d in result)
            {
                d.Should().Be(value);
            }
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
