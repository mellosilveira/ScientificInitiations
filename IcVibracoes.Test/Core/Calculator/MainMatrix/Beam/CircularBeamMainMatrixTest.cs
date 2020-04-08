using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public class CircularBeamMainMatrixTest : BeamMainMatrixTest<CircularBeamMainMatrix, CircularProfile>
    {
        public CircularBeamMainMatrixTest()
        {
            // Values were calculated for the GeometricProperty unit test.
            base._beamArea = 1.95611266575769E-04;
            base._beamMomentOfInertia = 2.1603613923E-08;

            base._beam = new Beam<CircularProfile>
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
                Profile = new CircularProfile()
            };

            base._elementMassMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];
            base._massMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base._elementHardnessMatrix = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];
            base._hardnessMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base._dampingMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
        }
    }
}
