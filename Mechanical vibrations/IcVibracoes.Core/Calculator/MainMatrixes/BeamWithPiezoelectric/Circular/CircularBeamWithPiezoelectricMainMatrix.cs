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
        /// <summary>
        /// This method calculates the element piezoelectric capacitance matrix.
        /// This method is not implemented.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="elementIndex"></param>
        /// <returns>The elementary piezoelectric capacitance matrix.</returns>
        public override Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<CircularProfile> beam, uint elementIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method calculates the electromechanical coupling matrix of an element of beam with piezoelectric plates.
        /// This method is not implemented.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The element's electromechanical coupling matrix.</returns>
        public override Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(
            BeamWithPiezoelectric<CircularProfile> beam)
        {
            throw new NotImplementedException();
        }
    }
}
