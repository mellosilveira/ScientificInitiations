using IcVibracoes.Calculator.MainMatrixes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beam;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamMainMatrix<TProfile> : CommonMainMatrix, IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
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
                double[,] elementMass = await base.CalculateElementMass(beam.GeometricProperty.Area[n], beam.Material.SpecificMass, length);

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
    }
}
