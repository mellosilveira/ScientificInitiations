﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations.ArrayOperations;
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
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public CircularBeamWithPiezoelectricMainMatrix(
            IArrayOperation arrayOperation) 
            : base(arrayOperation)
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
