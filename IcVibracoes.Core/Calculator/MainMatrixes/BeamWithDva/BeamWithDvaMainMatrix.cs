using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the beam with DVA main matrixes.
    /// </summary>
    public abstract class BeamWithDvaMainMatrix<TProfile> : BeamMainMatrix<TProfile>, IBeamWithDvaMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public BeamWithDvaMainMatrix(
            IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Responsible to calculate the mass matrix of the beam.
        /// </summary>
        /// <param name="beamMass"></param>
        /// <param name="dvaMasses"></param>
        /// <param name="dvaNodePositions"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateMassWithDva(double[,] beamMass, double[] dvaMasses, uint[] dvaNodePositions)
        {
            double[,] massWithDva = new double[beamMass.GetLength(0) + dvaMasses.Length, beamMass.GetLength(1) + dvaMasses.Length];

            beamMass = await this._arrayOperation.AddValue(beamMass, dvaMasses, dvaNodePositions, "Beam Mass").ConfigureAwait(false);

            for (int i = 0; i < beamMass.GetLength(0); i++)
            {
                for (int j = 0; j < beamMass.GetLength(1); j++)
                {
                    massWithDva[i, j] = beamMass[i, j];
                }
            }

            for (int i = 0; i < dvaMasses.Length; i++)
            {
                massWithDva[dvaNodePositions[i], dvaNodePositions[i]] = dvaMasses[i];
                massWithDva[i + beamMass.GetLength(0), i + beamMass.GetLength(0)] = dvaMasses[i];
            }

            return massWithDva;
        }

        /// <summary>
        /// Responsible to calculate the stiffness matrix of the beam.
        /// </summary>
        /// <param name="beamStiffness"></param>
        /// <param name="dvaStiffness"></param>
        /// <param name="dvaNodePositions"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateStiffnessWithDva(double[,] beamStiffness, double[] dvaStiffness, uint[] dvaNodePositions)
        {
            double[,] stiffnessWithDva = new double[beamStiffness.GetLength(0) + dvaStiffness.Length, beamStiffness.GetLength(1) + dvaStiffness.Length];

            beamStiffness = await this._arrayOperation.AddValue(beamStiffness, dvaStiffness, dvaNodePositions, "Beam Stiffness").ConfigureAwait(false);

            for (int i = 0; i < beamStiffness.GetLength(0); i++)
            {
                for (int j = 0; j < beamStiffness.GetLength(1); j++)
                {
                    stiffnessWithDva[i, j] = beamStiffness[i, j];
                }
            }

            for (int i = 0; i < dvaStiffness.Length; i++)
            {
                stiffnessWithDva[dvaNodePositions[i], dvaNodePositions[i]] += dvaStiffness[i];
                stiffnessWithDva[i + beamStiffness.GetLength(0), i + beamStiffness.GetLength(0)] = dvaStiffness[i];
                stiffnessWithDva[dvaNodePositions[i], i + beamStiffness.GetLength(0)] = -dvaStiffness[i];
                stiffnessWithDva[i + beamStiffness.GetLength(0), dvaNodePositions[i]] = -dvaStiffness[i];
            }

            return stiffnessWithDva;
        }
    }
}
