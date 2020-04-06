using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.BeamWithDvas
{
    public class CircularBeamWithDvaMainMatrixTest : BeamWithDvaMainMatrixTest<CircularBeamWithDvaMainMatrix, CircularProfile>
    {
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int DegreesFreedomMaximum = 6;
        private const int NumberOfDvas = 1;

        public CircularBeamWithDvaMainMatrixTest()
        {
            base.MassMatrix = new double[DegreesFreedomMaximum, DegreesFreedomMaximum];
            base.HardnessMatrix = new double[DegreesFreedomMaximum, DegreesFreedomMaximum];

            base.MassWithDvaMatrix = new double[DegreesFreedomMaximum + NumberOfDvas, DegreesFreedomMaximum + NumberOfDvas];
            base.HardnessWithDvaMatrix = new double[DegreesFreedomMaximum + NumberOfDvas, DegreesFreedomMaximum + NumberOfDvas];
        }
    }
}
