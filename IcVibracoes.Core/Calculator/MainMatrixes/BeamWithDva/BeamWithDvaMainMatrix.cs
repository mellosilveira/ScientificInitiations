using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the beam with DVA main matrixes.
    /// </summary>
    public abstract class BeamWithDvaMainMatrix<TProfile> : MainMatrix<BeamWithDva<TProfile>, TProfile>, IBeamWithDvaMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// This method calculates the mass matrix of beam with dynamic vibration absorber.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The strucutal mass matrix.</returns>
        public override double[,] CalculateMass(BeamWithDva<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] beamMass = base.CalculateMass(beam, degreesOfFreedom);

            double[,] massWithDva = new double[beamMass.GetLength(0) + beam.DvaMasses.Length, beamMass.GetLength(1) + beam.DvaMasses.Length];

            for (int i = 0; i < beamMass.GetLength(0); i++)
            {
                for (int j = 0; j < beamMass.GetLength(1); j++)
                {
                    massWithDva[i, j] = beamMass[i, j];
                }
            }

            for (int i = 0; i < beam.DvaMasses.Length; i++)
            {
                massWithDva[2 * beam.DvaNodePositions[i], 2 * beam.DvaNodePositions[i]] += beam.DvaMasses[i];
                massWithDva[i + beamMass.GetLength(0), i + beamMass.GetLength(0)] = beam.DvaMasses[i];
            }

            return massWithDva;
        }

        /// <summary>
        /// This method calculates the stiffness matrix of beam with dynamic vibration absorber.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The strucutal stiffness matrix.</returns>
        public override double[,] CalculateStiffness(BeamWithDva<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] beamStiffness = base.CalculateStiffness(beam, degreesOfFreedom);

            double[,] stiffnessWithDva = new double[beamStiffness.GetLength(0) + beam.DvaStiffnesses.Length, beamStiffness.GetLength(1) + beam.DvaStiffnesses.Length];

            for (int i = 0; i < beamStiffness.GetLength(0); i++)
            {
                for (int j = 0; j < beamStiffness.GetLength(1); j++)
                {
                    stiffnessWithDva[i, j] = beamStiffness[i, j];
                }
            }

            for (int i = 0; i < beam.DvaStiffnesses.Length; i++)
            {
                stiffnessWithDva[2 * beam.DvaNodePositions[i], 2 * beam.DvaNodePositions[i]] += beam.DvaStiffnesses[i];
                beamStiffness[beam.DvaNodePositions[i], i + beamStiffness.GetLength(0)] = -beam.DvaStiffnesses[i];
                beamStiffness[i + beamStiffness.GetLength(0), beam.DvaNodePositions[i]] = -beam.DvaStiffnesses[i];
                beamStiffness[i + beamStiffness.GetLength(0), i + beamStiffness.GetLength(0)] = beam.DvaStiffnesses[i];
            }

            return beamStiffness;
        }

        /// <summary>
        /// This method builds the boundary condition matrix and the number of true boundary conditions.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The boundary conditions matrix and the number of true boundary conditions.</returns>
        public override (bool[], uint) CalculateBoundaryConditions(BeamWithDva<TProfile> beam, uint degreesOfFreedom)
        {
            uint numberOfDvas = (uint)beam.DvaNodePositions.Length;

            (bool[] boundaryCondition, uint numberOfTrueBoundaryConditions) = base.CalculateBoundaryConditions(beam, degreesOfFreedom + numberOfDvas);

            return (boundaryCondition, numberOfTrueBoundaryConditions);
        }
    }
}
