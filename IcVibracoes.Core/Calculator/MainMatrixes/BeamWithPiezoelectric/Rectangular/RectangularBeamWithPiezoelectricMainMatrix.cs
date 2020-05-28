using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beams;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the rectangular beam with piezoelectric main matrixes.
    /// </summary>
    public class RectangularBeamWithPiezoelectricMainMatrix : BeamWithPiezoelectricMainMatrix<RectangularProfile>, IRectangularBeamWithPiezoelectricMainMatrix
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public RectangularBeamWithPiezoelectricMainMatrix(
            IArrayOperation arrayOperation)
            : base(arrayOperation) 
        { }

        public override Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<RectangularProfile> beamWithPiezoelectric, uint elementIndex)
        {
            double[,] piezoelectricCapacitance = new double[Constant.PiezoelectricDegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement];
            double elementLength = beamWithPiezoelectric.Length / beamWithPiezoelectric.NumberOfElements;

            double constant = -beamWithPiezoelectric.DielectricConstant * beamWithPiezoelectric.PiezoelectricGeometricProperty.Area[elementIndex] * elementLength / Math.Pow(beamWithPiezoelectric.PiezoelectricProfile.Height, 2);

            piezoelectricCapacitance[0, 0] = constant;
            piezoelectricCapacitance[0, 1] = -constant;
            piezoelectricCapacitance[1, 0] = -constant;
            piezoelectricCapacitance[1, 1] = constant;

            return Task.FromResult(piezoelectricCapacitance);
        }

        public override Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<RectangularProfile> beamWithPiezoelectric)
        {
            double[,] electromechanicalCoupling = new double[Constant.DegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement];
            double elementLength = beamWithPiezoelectric.Length / beamWithPiezoelectric.NumberOfElements;

            double constant = -(beamWithPiezoelectric.DielectricPermissiveness * beamWithPiezoelectric.PiezoelectricProfile.Width * elementLength / 2) * (2 * beamWithPiezoelectric.Profile.Height * beamWithPiezoelectric.PiezoelectricProfile.Height + Math.Pow(beamWithPiezoelectric.PiezoelectricProfile.Height, 2));

            electromechanicalCoupling[0, 0] = 0;
            electromechanicalCoupling[0, 1] = 0;
            electromechanicalCoupling[1, 0] = -elementLength * constant;
            electromechanicalCoupling[1, 1] = elementLength * constant;
            electromechanicalCoupling[2, 0] = 0;
            electromechanicalCoupling[2, 1] = elementLength * constant;
            electromechanicalCoupling[3, 0] = elementLength * constant;
            electromechanicalCoupling[3, 1] = -elementLength * constant;

            return Task.FromResult(electromechanicalCoupling);
        }
    }
}
