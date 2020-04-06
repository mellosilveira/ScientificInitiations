using FluentAssertions;
using IcVibracoes.Calculator.GeometricProperties;
using System;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator
{
    public class CalculateGeometricPropertyTest
    {
        private readonly CalculateGeometricProperty _operation;
        private readonly double _circleWithThicknessArea;
        private readonly double _circleWithoutThicknessArea;
        private readonly double _circleWithThicknessMomentOfInertia;
        private readonly double _circleWithoutThicknessMomentOfInertia;
        private readonly double _rectangleWithThicknessArea;
        private readonly double _rectangleWithoutThicknessArea;
        private readonly double _rectangleWithThicknessMomentOfInertia;
        private readonly double _rectangleWithoutThicknessMomentOfInertia;
        private readonly double _rectanglePiezoelectricMomentOfInertia;
        private readonly double _precision;
        private readonly double _diameter;
        private readonly double _width;
        private readonly double _height;
        private readonly double _piezoelectricHeight;
        private readonly double _diameterThickness;
        private readonly double _rectangularThickness;

        public CalculateGeometricPropertyTest()
        {
            this._operation = new CalculateGeometricProperty();

            this._precision = 1e-18;

            this._diameter = 0.03175;
            this._width = 25e-3;
            this._height = 3e-3;
            this._piezoelectricHeight = 0.267e-3;
            this._diameterThickness = 2.1e-3;
            this._rectangularThickness = 0.5e-3;

            // The calculations were made manually with the values inside that test.
            this._circleWithThicknessArea = 1.95611266575769E-04;
            this._circleWithoutThicknessArea = 7.91730436089840E-04;
            this._circleWithThicknessMomentOfInertia = 2.1603613923E-08;
            this._circleWithoutThicknessMomentOfInertia = 4.9882110171E-08;
            this._rectangleWithThicknessArea = 2.7000000000000E-05;
            this._rectangleWithoutThicknessArea = 7.5000000000000E-05;
            this._rectangleWithThicknessMomentOfInertia = 4.0250000E-11;
            this._rectangleWithoutThicknessMomentOfInertia = 5.6250000E-11;
            this._rectanglePiezoelectricMomentOfInertia = 3.5701411E-11;
        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Diameter. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateArea_Given_Diameter_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateArea(this._diameter, null);

            // Assert
            result.Should().BeApproximately(this._circleWithoutThicknessArea, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Diameter and thickness. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateArea_Given_DiameterAndThickness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateArea(this._diameter, this._diameterThickness);

            // Assert
            result.Should().BeApproximately(this._circleWithThicknessArea, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | Given: Diameter. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMomentOfInertia_Given_Diameter_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMomentOfInertia(this._diameter, null);

            // Assert
            result.Should().BeApproximately(this._circleWithoutThicknessMomentOfInertia, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | Given: Diameter and thickness. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMomentOfInertia_Given_DiameterAndThickness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMomentOfInertia(this._diameter, this._diameterThickness);

            // Assert
            result.Should().BeApproximately(this._circleWithThicknessMomentOfInertia, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Height and width. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateArea_Given_HeightAndWidth_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateArea(this._height, this._width, null);

            // Assert
            result.Should().BeApproximately(this._rectangleWithoutThicknessArea, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Heigth, width and thickness. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateArea_Given_HeightAndWidthAndThickness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateArea(this._height, this._width, this._rectangularThickness);

            // Assert
            result.Should().BeApproximately(this._rectangleWithThicknessArea, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | Given: Height and width. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMomentOfInertia_Given_HeightAndWidth_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMomentOfInertia(this._height, this._width, null);

            // Assert
            result.Should().BeApproximately(this._rectangleWithoutThicknessMomentOfInertia, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculateMomentOfInertia | Given: Heigth, width and thickness. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculateMomentOfInertia_Given_HeightAndWidthAndThickness_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculateMomentOfInertia(this._height, this._width, this._rectangularThickness);

            // Assert
            result.Should().BeApproximately(this._rectangleWithThicknessMomentOfInertia, this._precision);
        }

        [Fact(DisplayName = @"Feature: CalculatePiezoelectricMomentOfInertia | Given: Valid parameter. | When: Invoke. | Should: Execute correctly.")]
        public async void CalculatePiezoelectricMomentOfInertia_Should_ExecuteCorrectly()
        {
            // Act
            var result = await this._operation.CalculatePiezoelectricMomentOfInertia(this._piezoelectricHeight, this._width, this._height, numberOfPiezoelectricsPerElement: 2);

            // Assert
            result.Should().BeApproximately(this._rectanglePiezoelectricMomentOfInertia, this._precision);
        }
    }
}
