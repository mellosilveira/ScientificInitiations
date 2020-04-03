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
        private readonly double _rectangleWithThicknessArea;
        private readonly double _rectangleWithoutThicknessArea;
        private readonly double _circleWithThicknessMomentOfInertia;
        private readonly double _circleWithoutThicknessMomentOfInertia;
        private readonly double _rectangleWithThicknessMomentOfInertia;
        private readonly double _rectangleWithoutThicknessMomentOfInertia;

        private double _diameter;
        private double _width;
        private double _height;
        private double _thickness;

        public CalculateGeometricPropertyTest()
        {
            this._operation = new CalculateGeometricProperty();

            this._diameter = 0.03175;
            this._width = 0.010;
            this._height = 0.020;
            this._thickness = 0.002;

            // The calculations were made manually with the values inside that test.
            this._circleWithThicknessArea = 1.86924762888593e-04;
            this._circleWithoutThicknessArea = 7.91730436089840e-04;
            this._circleWithThicknessMomentOfInertia = 2.0773474626e-08;
            this._circleWithoutThicknessMomentOfInertia = 4.9882110171e-08;
            this._rectangleWithThicknessArea = 9.6000000000000e-05; 
            this._rectangleWithoutThicknessArea = 2.00000000000000e-04;
            this._rectangleWithThicknessMomentOfInertia = 2.048000000e-09;
            this._rectangleWithoutThicknessMomentOfInertia = 6.66666667e-09;

        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Invalid parameters. | When: Invoke. | Should: Throw ArgumentOutOfRangeException.")]
        public void CalculateArea_Should_ArgumentOutOfRangeException()
        {
            
        }
    }
}
