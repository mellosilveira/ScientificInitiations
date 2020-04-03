using IcVibracoes.Calculator.GeometricProperties;
using Xunit;

namespace IcVibracoes.Test.Core.Calculator
{
    public class CalculateGeometricPropertyTest
    {
        private readonly CalculateGeometricProperty _operation;

        public CalculateGeometricPropertyTest()
        {
            this._operation = new CalculateGeometricProperty();
        }

        [Fact(DisplayName = @"Feature: CalculateArea | Given: Valid parameters. | When: Invoke. | Should: Execute correctly.")]
        public void CalculateArea_Should_ExecuteCorrectly()
        {

        }
    }
}
