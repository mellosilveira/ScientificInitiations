using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular
{
    /// <summary>
    /// It's responsible to calculate the circular beam with piezoelectric main matrixes.
    /// </summary>
    public class CircularBeamWithPiezoelectricMainMatrix : BeamWithPiezoelectricMainMatrix<CircularProfile>, ICircularBeamWithPiezoelectricMainMatrix
    {
        public override Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<CircularProfile> beam, uint elementIndex)
        {
            throw new NotImplementedException();
        }

        public override Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<CircularProfile> beam)
        {
            throw new NotImplementedException();
        }
    }
}
