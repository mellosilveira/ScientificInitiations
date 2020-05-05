using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamMainMatrix<TProfile> : IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// It's responsible to calculate the element mass matrix.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="specificMass"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateElementMass(double area, double specificMass, double elementLength)
        {
            double constant = area * specificMass * elementLength / 420;

            double[,] elementMass = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];
            elementMass[0, 0] = 156 * constant;
            elementMass[0, 1] = 22 * elementLength * constant;
            elementMass[0, 2] = 54 * constant;
            elementMass[0, 3] = -13 * elementLength * constant;
            elementMass[1, 0] = 22 * elementLength * constant;
            elementMass[1, 1] = 4 * Math.Pow(elementLength, 2) * constant;
            elementMass[1, 2] = 13 * elementLength * constant;
            elementMass[1, 3] = -3 * Math.Pow(elementLength, 2) * constant;
            elementMass[2, 0] = 54 * constant;
            elementMass[2, 1] = 13 * elementLength * constant;
            elementMass[2, 2] = 156 * constant;
            elementMass[2, 3] = -22 * elementLength * constant;
            elementMass[3, 0] = -13 * elementLength * constant;
            elementMass[3, 1] = -3 * Math.Pow(elementLength, 2) * constant;
            elementMass[3, 2] = -22 * elementLength * constant;
            elementMass[3, 3] = 4 * Math.Pow(elementLength, 2) * constant;

            return Task.FromResult(elementMass);
        }

        /// <summary>
        /// Responsible to calculate the mass matrix of the beam.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateMass(Beam<TProfile> beam, uint degreesFreedomMaximum)
        {
            uint numberOfElements = beam.NumberOfElements;
            uint dfe = Constant.DegreesFreedomElement;

            double[,] mass = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double length = beam.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] elementMass = await this.CalculateElementMass(beam.GeometricProperty.Area[n], beam.Material.SpecificMass, length).ConfigureAwait(false);

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = (dfe / 2) * n; j < (dfe / 2) * n + dfe; j++)
                    {
                        mass[i, j] += elementMass[i - (dfe / 2) * n, j - (dfe / 2) * n];
                    }
                }
            }

            return mass;
        }

        /// <summary>
        /// Responsible to calculate the stiffness matrix of the beam element.
        /// </summary>
        /// <param name="momentOfInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateElementStiffness(double momentOfInertia, double youngModulus, double elementLength)
        {
            double[,] elementStiffness = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];

            double constant = momentOfInertia * youngModulus / Math.Pow(elementLength, 3);

            elementStiffness[0, 0] = 12 * constant;
            elementStiffness[0, 1] = 6 * elementLength * constant;
            elementStiffness[0, 2] = -12 * constant;
            elementStiffness[0, 3] = 6 * elementLength * constant;
            elementStiffness[1, 0] = 6 * elementLength * constant;
            elementStiffness[1, 1] = 4 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[1, 2] = -(6 * elementLength * constant);
            elementStiffness[1, 3] = 2 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[2, 0] = -(12 * constant);
            elementStiffness[2, 1] = -(6 * elementLength * constant);
            elementStiffness[2, 2] = 12 * constant;
            elementStiffness[2, 3] = -(6 * elementLength * constant);
            elementStiffness[3, 0] = 6 * elementLength * constant;
            elementStiffness[3, 1] = 2 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[3, 2] = -(6 * elementLength * constant);
            elementStiffness[3, 3] = 4 * Math.Pow(elementLength, 2) * constant;

            return Task.FromResult(elementStiffness);
        }

        /// <summary>
        /// Responsible to calculate the stiffness matrix of the beam.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateStiffness(Beam<TProfile> beam, uint degreesFreedomMaximum)
        {
            uint numberOfElements = beam.NumberOfElements;
            uint dfe = Constant.DegreesFreedomElement;

            double[,] stiffness = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double length = beam.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] elementStiffness = await this.CalculateElementStiffness(beam.GeometricProperty.MomentOfInertia[n], beam.Material.YoungModulus, length).ConfigureAwait(false);

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = (dfe / 2) * n; j < (dfe / 2) * n + dfe; j++)
                    {
                        stiffness[i, j] += elementStiffness[i - (dfe / 2) * n, j - (dfe / 2) * n];
                    }
                }
            }

            return stiffness;
        }

        /// <summary>
        /// It's responsible to calculate the damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateDamping(double[,] mass, double[,] stiffness)
        {
            int massRow = mass.GetLength(0);
            int massColumn = mass.GetLength(1);
            int stiffnessRow = mass.GetLength(0);
            int stiffnessColumn = mass.GetLength(1);

            if (massRow != massColumn)
            {
                throw new Exception($"Mass must be a square matrix.");
            }

            if (stiffnessRow != stiffnessColumn)
            {
                throw new Exception($"Stiffness must be a square matrix.");
            }

            if (massRow != stiffnessRow || massColumn != stiffnessColumn)
            {
                throw new Exception($"Mass sizes: {massRow}x{massColumn} must be equals to stiffness sizes: {stiffnessRow}x{stiffnessColumn}.");
            }

            int size = massRow;

            double[,] damping = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    damping[i, j] = Constant.Mi * mass[i, j] + Constant.Alpha * stiffness[i, j];
                }
            }

            return Task.FromResult(damping);
        }

        /// <summary>
        /// It's rewsponsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="fastenings"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public Task<bool[]> CalculateBondaryCondition(IDictionary<uint, FasteningType> fastenings, uint degreesOfFreedom)
        {
            bool[] boundaryCondition = new bool[degreesOfFreedom];

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                boundaryCondition[i] = true;
            }

            foreach (KeyValuePair<uint, FasteningType> fastening in fastenings)
            {
                boundaryCondition[2 * fastening.Key] = fastening.Value.LinearDisplacement;
                boundaryCondition[2 * fastening.Key + 1] = fastening.Value.AngularDisplacement;
            }

            return Task.FromResult(boundaryCondition);
        }
    }
}
