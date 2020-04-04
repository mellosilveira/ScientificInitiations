using FluentAssertions;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;
using Moq;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public abstract class BeamMainMatrixTest<TProfile>
        where TProfile : Profile, new()
    {
        private readonly Mock<BeamMainMatrix<TProfile>> _operation;
        private readonly double _elementLength;
        private readonly double _area;
        private readonly double _momentOfInertia;
        private readonly double[] _forces;
        private readonly Beam<TProfile> _beam;
        private readonly double[,] _elementMassMatrix;
        private readonly double[,] _massMatrix;
        private readonly double[,] _elementHardnessMatrix;
        private readonly double[,] _hardnessMatrix;

        private const int numberOfElements = 2;
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int degreesFreedomMaximum = 6;

        public BeamMainMatrixTest(double area, double momentOfInertia)
        {
            this._operation = new Mock<BeamMainMatrix<TProfile>>();

            this._area = area;
            this._momentOfInertia = momentOfInertia;

            this._elementLength = 0.5;

            this._forces = new double[degreesFreedomMaximum] { 0, 0, 100, 0, 0, 0 };

            this._beam = new Beam<TProfile>
            {
                FirstFastening = new Pinned(),
                Forces = this._forces,
                GeometricProperty = new GeometricProperty
                {
                    Area = new double[numberOfElements] { this._area, this._area },
                    MomentOfInertia = new double[numberOfElements] { this._momentOfInertia, this._momentOfInertia }
                },
                LastFastening = new Pinned(),
                Length = this._elementLength * numberOfElements,
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                Profile = new TProfile()
            };
        }

        public async void CalculateElementMass_Should_ExecuteCorrectly()
        {
            // Act 
            var result = await this._operation.Object.CalculateElementMass(this._area, this._beam.Material.SpecificMass, this._elementLength);

            // Assert
            result.Should();
        }
    }
}
