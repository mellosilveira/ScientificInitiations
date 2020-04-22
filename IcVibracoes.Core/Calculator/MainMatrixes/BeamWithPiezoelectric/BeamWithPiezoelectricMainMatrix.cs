using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beams;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the beam with piezoelectric main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamWithPiezoelectricMainMatrix<TProfile> : BeamMainMatrix<TProfile>, IBeamWithPiezoelectricMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public BeamWithPiezoelectricMainMatrix(IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// It's responsible to calculate piezoelectric mass matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateMass(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum)
        {
            uint numberOfElements = beamWithPiezoelectric.NumberOfElements;
            uint dfe = Constant.DegreesFreedomElement;

            double[,] mass = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double elementLength = beamWithPiezoelectric.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] elementPiezoelectricMass = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];
                double[,] elementBeamMass = await base.CalculateElementMass(beamWithPiezoelectric.GeometricProperty.Area[n], beamWithPiezoelectric.Material.SpecificMass, elementLength);

                if (beamWithPiezoelectric.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    elementPiezoelectricMass = await base.CalculateElementMass(beamWithPiezoelectric.PiezoelectricGeometricProperty.Area[n], beamWithPiezoelectric.PiezoelectricSpecificMass, elementLength);
                }

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = (dfe / 2) * n; j < (dfe / 2) * n + dfe; j++)
                    {
                        mass[i, j] += elementPiezoelectricMass[i - (dfe / 2) * n, j - (dfe / 2) * n] + elementBeamMass[i - (dfe / 2) * n, j - (dfe / 2) * n];
                    }
                }
            }

            return mass;
        }

        /// <summary>
        /// It's responsible to calculate piezoelectric stiffness matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateStiffness(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum)
        {
            uint numberOfElements = beamWithPiezoelectric.NumberOfElements;
            uint dfe = Constant.DegreesFreedomElement;

            double[,] stiffness = new double[degreesFreedomMaximum, degreesFreedomMaximum];

            double elementLength = beamWithPiezoelectric.Length / numberOfElements;

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] piezoelectricElementStiffness = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];
                double[,] beamElementStiffness = await base.CalculateElementStiffness(beamWithPiezoelectric.GeometricProperty.MomentOfInertia[n], beamWithPiezoelectric.Material.YoungModulus, elementLength);

                if (beamWithPiezoelectric.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementStiffness = await this.CalculatePiezoelectricElementStiffness(beamWithPiezoelectric.ElasticityConstant, beamWithPiezoelectric.PiezoelectricGeometricProperty.MomentOfInertia[n], elementLength);
                }

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = (dfe / 2) * n; j < (dfe / 2) * n + dfe; j++)
                    {
                        stiffness[i, j] += beamElementStiffness[i - (dfe / 2) * n, j - (dfe / 2) * n] + piezoelectricElementStiffness[i - (dfe / 2) * n, j - (dfe / 2) * n];
                    }
                }
            }

            return stiffness;
        }

        /// <summary>
        /// It's responsible to calculate piezoelectric element stiffness matrix.
        /// </summary>
        /// <param name="elasticityConstant"></param>
        /// <param name="momentOfInertia"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Task<double[,]> CalculatePiezoelectricElementStiffness(double elasticityConstant, double momentOfInertia, double length)
        {
            double[,] elementStiffness = new double[Constant.DegreesFreedomElement, Constant.DegreesFreedomElement];

            double constant = momentOfInertia * elasticityConstant / Math.Pow(length, 3);

            elementStiffness[0, 0] = 12 * constant;
            elementStiffness[0, 1] = 6 * length * constant;
            elementStiffness[0, 2] = -12 * constant;
            elementStiffness[0, 3] = 6 * length * constant;
            elementStiffness[1, 0] = 6 * length * constant;
            elementStiffness[1, 1] = 4 * Math.Pow(length, 2) * constant;
            elementStiffness[1, 2] = -(6 * length * constant);
            elementStiffness[1, 3] = 2 * Math.Pow(length, 2) * constant;
            elementStiffness[2, 0] = -(12 * constant);
            elementStiffness[2, 1] = -(6 * length * constant);
            elementStiffness[2, 2] = 12 * constant;
            elementStiffness[2, 3] = -(6 * length * constant);
            elementStiffness[3, 0] = 6 * length * constant;
            elementStiffness[3, 1] = 2 * Math.Pow(length, 2) * constant;
            elementStiffness[3, 2] = -(6 * length * constant);
            elementStiffness[3, 3] = 4 * Math.Pow(length, 2) * constant;

            return Task.FromResult(elementStiffness);
        }

        /// <summary>
        /// It's responsible to calculate piezoelectric electromechanical coupling matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculatePiezoelectricElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint degreesFreedomMaximum)
        {
            uint numberOfNodes = beamWithPiezoelectric.NumberOfElements + 1;
            uint numberOfElements = beamWithPiezoelectric.NumberOfElements;
            uint dfe = Constant.DegreesFreedomElement;
            double[,] piezoelectricElectromechanicalCoupling = new double[degreesFreedomMaximum, numberOfNodes];

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] piezoelectricElementElectromechanicalCoupling = new double[Constant.DegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement];

                if (beamWithPiezoelectric.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementElectromechanicalCoupling = await this.CalculatePiezoelectricElementElectromechanicalCoupling(beamWithPiezoelectric);
                }

                for (uint i = (dfe / 2) * n; i < (dfe / 2) * n + dfe; i++)
                {
                    for (uint j = n; j < n + Constant.PiezoelectricDegreesFreedomElement; j++)
                    {
                        piezoelectricElectromechanicalCoupling[i, j] += piezoelectricElementElectromechanicalCoupling[i - 2 * n, j - n];
                    }
                }
            }

            return piezoelectricElectromechanicalCoupling;
        }

        /// <summary>
        /// It's responsible to calculate piezoelectric element electromechanical coupling matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <returns></returns>
        public abstract Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric);

        /// <summary>
        /// It's responsible to calculate piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculatePiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric)
        {
            uint numberOfElements = beamWithPiezoelectric.NumberOfElements;
            double[,] piezoelectricCapacitance = new double[numberOfElements + 1, numberOfElements + 1];

            for (uint n = 0; n < numberOfElements; n++)
            {
                double[,] piezoelectricElementCapacitance = new double[Constant.PiezoelectricDegreesFreedomElement, Constant.PiezoelectricDegreesFreedomElement];

                if (beamWithPiezoelectric.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementCapacitance = await this.CalculateElementPiezoelectricCapacitance(beamWithPiezoelectric, elementIndex: n);
                }

                for (uint i = n; i < n + Constant.PiezoelectricDegreesFreedomElement; i++)
                {
                    for (uint j = n; j < n + Constant.PiezoelectricDegreesFreedomElement; j++)
                    {
                        piezoelectricCapacitance[i, j] += piezoelectricElementCapacitance[i - n, j - n];
                    }
                }
            }

            return piezoelectricCapacitance;
        }

        /// <summary>
        /// It's responsible to calculate element piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beamWithPiezoelectric"></param>
        /// <param name="elementIndex"></param>
        /// <returns></returns>
        public abstract Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beamWithPiezoelectric, uint elementIndex);

        /// <summary>
        /// It's responsible to calculate equivalent mass matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <param name="piezoelectricDegreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentMass(double[,] mass, uint degreesFreedomMaximum, uint piezoelectricDegreesFreedomMaximum)
        {
            uint matrixSize = degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum;
            double[,] equivalentMass = new double[matrixSize, matrixSize];

            for (uint i = 0; i < matrixSize; i++)
            {
                for (uint j = 0; j < matrixSize; j++)
                {
                    if (i < degreesFreedomMaximum && j < degreesFreedomMaximum)
                    {
                        equivalentMass[i, j] = mass[i, j];
                    }
                    else
                    {
                        equivalentMass[i, j] = 0;
                    }
                }
            }

            return Task.FromResult(equivalentMass);
        }

        /// <summary>
        /// It's responsible to calculate equivalent stiffness matrix.
        /// </summary>
        /// <param name="stiffness"></param>
        /// <param name="piezoelectricElectromechanicalCoupling"></param>
        /// <param name="piezoelectricCapacitance"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <param name="piezoelectricDegreesFreedomMaximum"></param>
        /// <returns></returns>
        public async Task<double[,]> CalculateEquivalentStiffness(double[,] stiffness, double[,] piezoelectricElectromechanicalCoupling, double[,] piezoelectricCapacitance, uint degreesFreedomMaximum, uint piezoelectricDegreesFreedomMaximum)
        {
            uint matrixSize = degreesFreedomMaximum + piezoelectricDegreesFreedomMaximum;

            double[,] piezoelectricElectromechanicalCouplingTransposed = await this._arrayOperation.TransposeMatrix(piezoelectricElectromechanicalCoupling);

            double[,] equivalentStiffness = new double[matrixSize, matrixSize];

            for (uint i = 0; i < matrixSize; i++)
            {
                for (uint j = 0; j < matrixSize; j++)
                {
                    if (i < degreesFreedomMaximum && j < degreesFreedomMaximum)
                    {
                        equivalentStiffness[i, j] = stiffness[i, j];
                    }
                    else if (i < degreesFreedomMaximum && j >= degreesFreedomMaximum)
                    {
                        equivalentStiffness[i, j] = piezoelectricElectromechanicalCoupling[i, j - degreesFreedomMaximum];
                    }
                    else if (i >= degreesFreedomMaximum && j < degreesFreedomMaximum)
                    {
                        equivalentStiffness[i, j] = piezoelectricElectromechanicalCouplingTransposed[i - degreesFreedomMaximum, j];
                    }
                    else if (i >= degreesFreedomMaximum && j >= degreesFreedomMaximum)
                    {
                        equivalentStiffness[i, j] = piezoelectricCapacitance[i - degreesFreedomMaximum, j - degreesFreedomMaximum];
                    }
                }
            }

            return equivalentStiffness;
        }

        /// <summary>
        /// It's responsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <returns></returns>
        public Task<bool[]> CalculatePiezoelectricBondaryCondition(uint numberOfElements, uint[] elementsWithPiezoelectric)
        {
            bool[] bondaryCondition = new bool[numberOfElements + 1];

            for (uint i = 0; i < numberOfElements; i++)
            {
                if (elementsWithPiezoelectric.Contains(i + 1))
                {
                    bondaryCondition[i] = true;
                    bondaryCondition[i + 1] = true;
                }
                else
                {
                    bondaryCondition[i] = false;
                    bondaryCondition[i + 1] = false;
                }
            }

            return Task.FromResult(bondaryCondition);
        }
    }
}
