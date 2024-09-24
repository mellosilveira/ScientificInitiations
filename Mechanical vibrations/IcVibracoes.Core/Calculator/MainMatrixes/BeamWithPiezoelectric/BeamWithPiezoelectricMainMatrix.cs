﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the beam with piezoelectric main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class BeamWithPiezoelectricMainMatrix<TProfile> : MainMatrix<BeamWithPiezoelectric<TProfile>, TProfile>, IBeamWithPiezoelectricMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// This method calculates the mass matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure mass matrix.</returns>
        public async Task<double[,]> CalculateStructureMass(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] mass = new double[degreesOfFreedom, degreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double elementLength = beam.Length / beam.NumberOfElements;

                double[,] elementBeamMass = await base.CalculateElementMass(beam.GeometricProperty.Area[n], beam.Material.SpecificMass, elementLength).ConfigureAwait(false);
                double[,] elementPiezoelectricMass = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];

                if (beam.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    elementPiezoelectricMass = await base.CalculateElementMass(beam.PiezoelectricGeometricProperty.Area[n], beam.PiezoelectricSpecificMass, elementLength).ConfigureAwait(false);
                }

                for (uint i = 2 * n; i < 2 * n + Constants.DegreesOfFreedomElement; i++)
                {
                    for (uint j = 2 * n; j < 2 * n + Constants.DegreesOfFreedomElement; j++)
                    {
                        mass[i, j] += elementPiezoelectricMass[i - 2 * n, j - 2 * n] + elementBeamMass[i - 2 * n, j - 2 * n];
                    }
                }
            }

            return mass;
        }

        /// <summary>
        /// This method calculates the equivalent mass matrix to be used in Finite Element Analysis.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The equivalent mass matrix.</returns>
        public override async Task<double[,]> CalculateMassAsync(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] mass = await this.CalculateStructureMass(beam, degreesOfFreedom).ConfigureAwait(false);

            uint matrixSize = degreesOfFreedom + beam.PiezoelectricDegreesOfFreedom;

            double[,] equivalentMass = new double[matrixSize, matrixSize];

            for (uint i = 0; i < matrixSize; i++)
            {
                for (uint j = 0; j < matrixSize; j++)
                {
                    if (i < degreesOfFreedom && j < degreesOfFreedom)
                    {
                        equivalentMass[i, j] = mass[i, j];
                    }
                    else
                    {
                        equivalentMass[i, j] = 0;
                    }
                }
            }

            return equivalentMass;
        }

        /// <summary>
        /// This method calculates stiffness matrix of an element of beam with piezoelectric plates.
        /// </summary>
        /// <param name="elasticityConstant"></param>
        /// <param name="momentOfInertia"></param>
        /// <param name="elementLength"></param>
        /// <returns>The element's piezoelectric stiffness matrix.</returns>
        public Task<double[,]> CalculatePiezoelectricElementStiffness(double elasticityConstant, double momentOfInertia, double elementLength)
        {
            double constant = momentOfInertia * elasticityConstant / Math.Pow(elementLength, 3);

            double[,] elementStiffness = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];
            elementStiffness[0, 0] = 12 * constant;
            elementStiffness[0, 1] = 6 * elementLength * constant;
            elementStiffness[0, 2] = -12 * constant;
            elementStiffness[0, 3] = 6 * elementLength * constant;

            elementStiffness[1, 0] = 6 * elementLength * constant;
            elementStiffness[1, 1] = 4 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[1, 2] = -6 * elementLength * constant;
            elementStiffness[1, 3] = 2 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[2, 0] = -12 * constant;
            elementStiffness[2, 1] = -6 * elementLength * constant;
            elementStiffness[2, 2] = 12 * constant;
            elementStiffness[2, 3] = -6 * elementLength * constant;
            elementStiffness[3, 0] = 6 * elementLength * constant;
            elementStiffness[3, 1] = 2 * Math.Pow(elementLength, 2) * constant;
            elementStiffness[3, 2] = -6 * elementLength * constant;
            elementStiffness[3, 3] = 4 * Math.Pow(elementLength, 2) * constant;

            return Task.FromResult(elementStiffness);
        }

        /// <summary>
        /// This method calculates the stiffness matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure stiffness matrix.</returns>
        public async Task<double[,]> CalculateStructureStiffness(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] stiffness = new double[degreesOfFreedom, degreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double elementLength = beam.Length / beam.NumberOfElements;

                double[,] piezoelectricElementStiffness = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];
                double[,] beamElementStiffness = await base.CalculateElementStiffness(beam.GeometricProperty.MomentOfInertia[n], beam.Material.YoungModulus, elementLength).ConfigureAwait(false);

                if (beam.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementStiffness = await this.CalculatePiezoelectricElementStiffness(beam.ElasticityConstant, beam.PiezoelectricGeometricProperty.MomentOfInertia[n], elementLength).ConfigureAwait(false);
                }

                for (uint i = 2 * n; i < 2 * n + Constants.DegreesOfFreedomElement; i++)
                {
                    for (uint j = 2 * n; j < 2 * n + Constants.DegreesOfFreedomElement; j++)
                    {
                        stiffness[i, j] += beamElementStiffness[i - 2 * n, j - 2 * n] + piezoelectricElementStiffness[i - 2 * n, j - 2 * n];
                    }
                }
            }

            return stiffness;
        }

        /// <summary>
        /// This method calculates the electromechanical coupling matrix of an element of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The element's electromechanical coupling matrix.</returns>
        public abstract Task<double[,]> CalculatePiezoelectricElementElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beam);

        /// <summary>
        /// This method calculates electromechanical coupling matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure piezoelectric electromechanical coupling matrix.</returns>
        public async Task<double[,]> CalculatePiezoelectricElectromechanicalCoupling(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] piezoelectricElectromechanicalCoupling = new double[degreesOfFreedom, beam.PiezoelectricDegreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double[,] piezoelectricElementElectromechanicalCoupling = new double[Constants.DegreesOfFreedomElement, Constants.PiezoelectricDegreesOfFreedomElement];

                if (beam.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementElectromechanicalCoupling = await this.CalculatePiezoelectricElementElectromechanicalCoupling(beam).ConfigureAwait(false);
                }

                for (uint i = 2 * n; i < 2 * n + Constants.DegreesOfFreedomElement; i++)
                {
                    for (uint j = n; j < n + Constants.PiezoelectricDegreesOfFreedomElement; j++)
                    {
                        piezoelectricElectromechanicalCoupling[i, j] += piezoelectricElementElectromechanicalCoupling[i - 2 * n, j - n];
                    }
                }
            }

            return piezoelectricElectromechanicalCoupling;
        }

        /// <summary>
        /// This method calculates the element piezoelectric capacitance matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="elementIndex"></param>
        /// <returns>The elementary piezoelectric capacitance matrix.</returns>
        public abstract Task<double[,]> CalculateElementPiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beam, uint elementIndex);

        /// <summary>
        /// This method calculates the piezoelectric capacitance matrix of beam with piezoelectric plates.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The structure piezoelectric capacitance matrix.</returns>
        public async Task<double[,]> CalculatePiezoelectricCapacitance(BeamWithPiezoelectric<TProfile> beam)
        {
            double[,] piezoelectricCapacitance = new double[beam.PiezoelectricDegreesOfFreedom, beam.PiezoelectricDegreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double[,] piezoelectricElementCapacitance = new double[Constants.PiezoelectricDegreesOfFreedomElement, Constants.PiezoelectricDegreesOfFreedomElement];

                if (beam.ElementsWithPiezoelectric.Contains(n + 1))
                {
                    piezoelectricElementCapacitance = await this.CalculateElementPiezoelectricCapacitance(beam, elementIndex: n).ConfigureAwait(false);
                }

                for (uint i = n; i < n + Constants.PiezoelectricDegreesOfFreedomElement; i++)
                {
                    for (uint j = n; j < n + Constants.PiezoelectricDegreesOfFreedomElement; j++)
                    {
                        piezoelectricCapacitance[i, j] += piezoelectricElementCapacitance[i - n, j - n];
                    }
                }
            }

            return piezoelectricCapacitance;
        }

        /// <summary>
        /// This method calculates the equivalent stiffness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The equivalent stiffness matrix.</returns>
        public override async Task<double[,]> CalculateStiffnessAsync(BeamWithPiezoelectric<TProfile> beam, uint degreesOfFreedom)
        {
            double[,] stiffness = await this.CalculateStructureStiffness(beam, degreesOfFreedom).ConfigureAwait(false);
            double[,] piezoelectricElectromechanicalCoupling = await this.CalculatePiezoelectricElectromechanicalCoupling(beam, degreesOfFreedom).ConfigureAwait(false);
            double[,] piezoelectricElectromechanicalCouplingTransposed = piezoelectricElectromechanicalCoupling.TransposeMatrixAsync();
            double[,] piezoelectricCapacitance = await this.CalculatePiezoelectricCapacitance(beam).ConfigureAwait(false);

            uint matrixSize = degreesOfFreedom + beam.PiezoelectricDegreesOfFreedom;

            double[,] equivalentStiffness = new double[matrixSize, matrixSize];

            for (uint i = 0; i < matrixSize; i++)
            {
                for (uint j = 0; j < matrixSize; j++)
                {
                    if (i < degreesOfFreedom && j < degreesOfFreedom)
                    {
                        equivalentStiffness[i, j] = stiffness[i, j];
                    }
                    else if (i < degreesOfFreedom && j >= degreesOfFreedom)
                    {
                        equivalentStiffness[i, j] = piezoelectricElectromechanicalCoupling[i, j - degreesOfFreedom];
                    }
                    else if (i >= degreesOfFreedom && j < degreesOfFreedom)
                    {
                        equivalentStiffness[i, j] = piezoelectricElectromechanicalCouplingTransposed[i - degreesOfFreedom, j];
                    }
                    else if (i >= degreesOfFreedom && j >= degreesOfFreedom)
                    {
                        equivalentStiffness[i, j] = piezoelectricCapacitance[i - degreesOfFreedom, j - degreesOfFreedom];
                    }
                }
            }

            return equivalentStiffness;
        }

        /// <summary>
        /// This method calculates the beam's force matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The structure force matrix.</returns>
        public override Task<double[]> CalculateForce(BeamWithPiezoelectric<TProfile> beam) => Task.FromResult(beam.Forces.CombineVectors(beam.ElectricalCharge));

        /// <summary>
        /// This method builds the boundary condition matrix and the number of true boundary conditions.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The boundary conditions matrix and the number of true boundary conditions.</returns>
        public override async Task<(bool[], uint)> CalculateBoundaryConditionsAsync(BeamWithPiezoelectric<TProfile> beam,
            uint degreesOfFreedom)
        {
            uint piezoelectricDegreesOfFreedom = beam.NumberOfElements + 1;

            // Creates the general boundary conditions vector with the piezoelectric all boundary conditions true.
            (bool[] boundaryCondition, uint numberOfTrueBoundaryConditions) = await base.CalculateBoundaryConditionsAsync(beam, degreesOfFreedom + piezoelectricDegreesOfFreedom).ConfigureAwait(false);

            // Sets the correct piezoelectric boundary conditions.
            foreach (KeyValuePair<uint, FasteningType> fastening in beam.Fastenings)
            {
                boundaryCondition[degreesOfFreedom + fastening.Key] = fastening.Value.AllowLinearDisplacement;

                if (fastening.Value.AllowLinearDisplacement == false)
                {
                    numberOfTrueBoundaryConditions -= 1;
                }
            }

            return (boundaryCondition, numberOfTrueBoundaryConditions);
        }
    }
}


