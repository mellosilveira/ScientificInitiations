using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Models;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.Beam
{
    public class CircularBeamMainMatrixTest : BeamMainMatrixTest<CircularBeamMainMatrix, CircularProfile>
    {
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int degreesFreedomMaximum = 6;

        public CircularBeamMainMatrixTest()
        {
            // Values were calculated for the GeometricProperty unit test.
            base.Area = 1.95611266575769E-04;
            base.MomentOfInertia = 2.1603613923E-08;

            base.ElementMassMatrix = new double[Constant.DegreesFreedom, Constant.DegreesFreedom];
            base.MassMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base.ElementHardnessMatrix = new double[Constant.DegreesFreedom, Constant.DegreesFreedom];
            base.HardnessMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
            base.DampingMatrix = new double[degreesFreedomMaximum, degreesFreedomMaximum];
        }
    }
}
