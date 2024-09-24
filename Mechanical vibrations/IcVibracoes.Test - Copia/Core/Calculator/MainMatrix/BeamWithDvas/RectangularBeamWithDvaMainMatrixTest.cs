using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;

namespace IcVibracoes.Test.Core.Calculator.MainMatrix.BeamWithDvas
{
    public class RectangularBeamWithDvaMainMatrixTest : BeamWithDvaMainMatrixTest<RectangularBeamWithDvaMainMatrix, RectangularProfile>
    {
        // Degrees Freedom Maximum = (Number of Elements + 1) * Degrees Freedom Per Node
        private const int DegreesFreedomMaximum = 6;
        private const int NumberOfDvas = 1;

        public RectangularBeamWithDvaMainMatrixTest()
        {
            base.MassMatrix = new double[DegreesFreedomMaximum, DegreesFreedomMaximum];
            base.StiffnessMatrix = new double[DegreesFreedomMaximum, DegreesFreedomMaximum];

            base.MassWithDvaMatrix = new double[DegreesFreedomMaximum + NumberOfDvas, DegreesFreedomMaximum + NumberOfDvas];
            base.StiffnessWithDvaMatrix = new double[DegreesFreedomMaximum + NumberOfDvas, DegreesFreedomMaximum + NumberOfDvas];
        }
    }
}
