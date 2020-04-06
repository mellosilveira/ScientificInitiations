using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Models;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public class RectangularBeamMainMatrixTest : BeamMainMatrixTest<RectangularBeamMainMatrix, RectangularProfile>
    {
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int degreesFreedomMaximum = 6;

        public RectangularBeamMainMatrixTest()
        {
            // Values were calculated for the GeometricProperty unit test.
            base.Area = 2.7E-05;
            base.MomentOfInertia = 4.025E-11;

            base.ElementMassMatrix = new double[Constant.DegreesFreedom, Constant.DegreesFreedom];
            base.MassMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base.ElementHardnessMatrix = new double[Constant.DegreesFreedom, Constant.DegreesFreedom];
            base.HardnessMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base.DampingMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
        }
    }
}
