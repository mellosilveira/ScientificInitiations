using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public class RectangularBeamMainMatrixTest : BeamMainMatrixTest<RectangularBeamMainMatrix, RectangularProfile>
    {
        public RectangularBeamMainMatrixTest()
        {
            // Values were calculated for the GeometricProperty unit test.
            base._beamArea = 7.5E-05;
            base._beamMomentOfInertia = 5.625E-11;

            base._beam = new Beam<RectangularProfile>
            {
                FirstFastening = new Pinned(),
                Forces = base._forceVector,
                GeometricProperty = new GeometricProperty
                {
                    Area = new double[numberOfElements] { this._beamArea, this._beamArea },
                    MomentOfInertia = new double[numberOfElements] { this._beamMomentOfInertia, this._beamMomentOfInertia }
                },
                LastFastening = new Pinned(),
                Length = base._elementLength * numberOfElements,
                Material = new Steel4130(),
                NumberOfElements = numberOfElements,
                Profile = new RectangularProfile()
            };

            base._elementMassMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement]
            {
                { 0.109339, 0.007710, 0.037848, -0.004556 },
                { 0.007710, 0.000701, 0.004556, -0.000526 },
                { 0.037848, 0.004556, 0.109339, -0.007710 },
                { -0.004556, -0.000526, -0.007710, 0.000701 }
            };

            base._massMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum]
            {
                { 0.109339, 0.007710, 0.037848, -0.004556, 0.000000, 0.000000 },
                { 0.007710, 0.000701, 0.004556, -0.000526, 0.000000, 0.000000 },
                { 0.037848, 0.004556, 0.218679, 0.000000, 0.037848, -0.004556 },
                { -0.004556, -0.000526, 0.000000, 0.001402, 0.004556, -0.000526 },
                { 0.000000, 0.000000, 0.037848, 0.004556, 0.109339, -0.007710 },
                { 0.000000, 0.000000, -0.004556, -0.000526, -0.007710, 0.000701 }
            };
            
            base._elementHardnessMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement]
            {
                { 1080.000000, 270.000000, -1080.000000, 270.000000 },
                { 270.000000, 90.000000, -270.000000, 45.000000 },
                { -1080.000000, -270.000000, 1080.000000, -270.000000 },
                { 270.000000, 45.000000, -270.000000, 90.000000 }
            };

            base._hardnessMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum]
            {
                { 1080.000000, 270.000000, -1080.000000, 270.000000, 0.000000, 0.000000 },
                { 270.000000, 90.000000, -270.000000, 45.000000, 0.000000, 0.000000 },
                { -1080.000000, -270.000000, 2160.000000, 0.000000, -1080.000000, 270.000000 },
                { 270.000000, 45.000000, 0.000000, 180.000000, -270.000000, 45.000000 },
                { 0.000000, 0.000000, -1080.000000, -270.000000, 1080.000000, -270.000000 },
                { 0.000000, 0.000000, 270.000000, 45.000000, -270.000000, 90.000000 }
            };
            
            base._dampingMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
        }
    }
}
