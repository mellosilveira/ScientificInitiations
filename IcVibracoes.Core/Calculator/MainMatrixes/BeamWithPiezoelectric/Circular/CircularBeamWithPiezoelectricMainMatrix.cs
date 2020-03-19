using IcVibracoes.Calculator.MainMatrixes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Models.Piezoelectric;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular
{
    /// <summary>
    /// It's responsible to calculate the circular beam with piezoelectric main matrixes.
    /// </summary>
    public class CircularBeamWithPiezoelectricMainMatrix : BeamWithPiezoelectricMainMatrix<CircularProfile>, ICircularBeamWithPiezoelectricMainMatrix
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="beamMainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CircularBeamWithPiezoelectricMainMatrix(
            ICircularBeamMainMatrix beamMainMatrix, 
            IArrayOperation arrayOperation) 
            : base(beamMainMatrix, arrayOperation)
        {
        }

        public override Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<CircularProfile> beamWithPiezoelectric, uint elementIndex)
        {
            throw new NotImplementedException();
        }

        public override Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<CircularProfile> beamWithPiezoelectric)
        {
            throw new NotImplementedException();
        }
    }
}
