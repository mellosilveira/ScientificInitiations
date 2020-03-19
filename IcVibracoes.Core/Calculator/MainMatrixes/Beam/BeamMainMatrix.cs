using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beam;
using IcVibracoes.Models.Beam.Characteristics;
using System;
using System.Threading.Tasks;

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
        /// <param name="length"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateElementMass(double area, double specificMass, double length)
        {
            double[,] elementMass = new double[Constants.DegreesFreedomElement, Constants.DegreesFreedomElement];

            double constant = area * specificMass * length / 420;

            elementMass[0, 0] = 156 * constant;
            elementMass[0, 1] = 22 * length * constant;
            elementMass[0, 2] = 54 * constant;
            elementMass[0, 3] = -13 * length * constant;
            elementMass[1, 0] = 22 * length * constant;
            elementMass[1, 1] = 4 * length * length * constant;
            elementMass[1, 2] = 13 * length * constant;
            elementMass[1, 3] = -3 * length * length * constant;
            elementMass[2, 0] = 54 * constant;
            elementMass[2, 1] = 13 * length * constant;
            elementMass[2, 2] = 156 * constant;
            elementMass[2, 3] = -22 * length * constant;
            elementMass[3, 0] = -13 * length * constant;
            elementMass[3, 1] = -3 * length * length * constant;
            elementMass[3, 2] = -22 * length * constant;
            elementMass[3, 3] = 4 * length * length * constant;

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
            uint dfe = Constants.DegreesFreedomElement;

            double[,] mass = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double length = beam.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] elementMass = await this.CalculateElementMass(beam.GeometricProperty.Area[n], beam.Material.SpecificMass, length);

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
        /// Responsible to calculate the hardness matrix of the beam element.
        /// </summary>
        /// <param name="momentOfInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateElementHardness(double momentOfInertia, double youngModulus, double length)
        {
            double[,] elementHardness = new double[Constants.DegreesFreedomElement, Constants.DegreesFreedomElement];

            double constant = momentOfInertia * youngModulus / Math.Pow(length, 3);

            elementHardness[0, 0] = 12 * constant;
            elementHardness[0, 1] = 6 * length * constant;
            elementHardness[0, 2] = -12 * constant;
            elementHardness[0, 3] = 6 * length * constant;
            elementHardness[1, 0] = 6 * length * constant;
            elementHardness[1, 1] = 4 * Math.Pow(length, 2) * constant;
            elementHardness[1, 2] = -(6 * length * constant);
            elementHardness[1, 3] = 2 * Math.Pow(length, 2) * constant;
            elementHardness[2, 0] = -(12 * constant);
            elementHardness[2, 1] = -(6 * length * constant);
            elementHardness[2, 2] = 12 * constant;
            elementHardness[2, 3] = -(6 * length * constant);
            elementHardness[3, 0] = 6 * length * constant;
            elementHardness[3, 1] = 2 * Math.Pow(length, 2) * constant;
            elementHardness[3, 2] = -(6 * length * constant);
            elementHardness[3, 3] = 4 * Math.Pow(length, 2) * constant;

            return Task.FromResult(elementHardness);
        }

        /// <summary>
        /// Responsible to calculate the hardness matrix of the beam.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateHardness(Beam<TProfile> beam, uint degreesFreedomMaximum)
        {
            uint numberOfElements = beam.NumberOfElements;
            uint dfe = Constants.DegreesFreedomElement;

            double[,] hardness = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double length = beam.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] elementHardness = await this.CalculateElementHardness(beam.GeometricProperty.MomentOfInertia[n], beam.Material.YoungModulus, length);

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = (dfe / 2) * n; j < (dfe / 2) * n + dfe; j++)
                    {
                        hardness[i, j] += elementHardness[i - (dfe / 2) * n, j - (dfe / 2) * n];
                    }
                }
            }

            return hardness;
        }

        /// <summary>
        /// It's responsible to calculate the damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="hardness"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateDamping(double[,] mass, double[,] hardness)
        {
            int massRow = mass.GetLength(0);
            int massColumn = mass.GetLength(1);
            int hardnessRow = mass.GetLength(0);
            int hardnessColumn = mass.GetLength(1);

            if (massRow != massColumn)
            {
                throw new Exception($"Mass must be a square matrix.");
            }

            if (hardnessRow != hardnessColumn)
            {
                throw new Exception($"Hardness must be a square matrix.");
            }

            if (massRow != hardnessRow || massColumn != hardnessColumn)
            {
                throw new Exception($"Mass sizes: {massRow}x{massColumn} must be equals to hardness sizes: {hardnessRow}x{hardnessColumn}.");
            }

            int size = massRow;

            double[,] damping = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    damping[i, j] = Constants.Mi * mass[i, j] + Constants.Alpha * hardness[i, j];
                }
            }

            return Task.FromResult(damping);
        }
        
        /// <summary>
        /// It's responsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="firstFastening"></param>
        /// <param name="lastFastening"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<bool[]> CalculateBondaryCondition(Fastening firstFastening, Fastening lastFastening, uint degreesFreedomMaximum)
        {
            bool[] bondaryCondition = new bool[degreesFreedomMaximum];

            for (uint i = 0; i < degreesFreedomMaximum; i++)
            {
                if (i == 0)
                {
                    bondaryCondition[i] = firstFastening.LinearDisplacement;
                }
                else if (i == degreesFreedomMaximum - 2)
                {
                    bondaryCondition[i] = lastFastening.LinearDisplacement;
                }
                else if (i == 1)
                {
                    bondaryCondition[i] = firstFastening.AngularDisplacement;
                }
                else if (i == degreesFreedomMaximum - 1)
                {
                    bondaryCondition[i] = lastFastening.AngularDisplacement;
                }
                else
                {
                    bondaryCondition[i] = true;
                }
            }

            return Task.FromResult(bondaryCondition);
        }
    }
}
