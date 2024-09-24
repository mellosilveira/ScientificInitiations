﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes
{
    /// <summary>
    /// It's responsible to calculate the main matrixes that wil bu used in Finite Element Analysis.
    /// </summary>
    /// <typeparam name="TBeam"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class MainMatrix<TBeam, TProfile> : IMainMatrix<TBeam, TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        /// <summary>
        /// This method calculates the element's mass matrix.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="specificMass"></param>
        /// <param name="elementLength"></param>
        /// <returns>The elementary mass matrix.</returns>
        public Task<double[,]> CalculateElementMass(double area, double specificMass, double elementLength)
        {
            double constant = area * specificMass * elementLength / 420;

            double[,] elementMass = new double[Constants.DegreesOfFreedomElement, Constants.DegreesOfFreedomElement];
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
        /// This method calculates the beam's mass matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure mass matrix.</returns>
        public virtual async Task<double[,]> CalculateMassAsync(TBeam beam, uint degreesOfFreedom)
        {
            double[,] mass = new double[degreesOfFreedom, degreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double length = beam.Length / beam.NumberOfElements;
                double[,] elementMass = await this.CalculateElementMass(beam.GeometricProperty.Area[n], beam.Material.SpecificMass, length).ConfigureAwait(false);

                for (uint i = 2 * n; i < 2 * n + Constants.DegreesOfFreedomElement; i++)
                {
                    for (uint j = 2 * n; j < 2 * n + Constants.DegreesOfFreedomElement; j++)
                    {
                        mass[i, j] += elementMass[i - 2 * n, j - 2 * n];
                    }
                }
            }

            return mass;
        }

        /// <summary>
        /// This method calculates the element's stiffness matrix.
        /// </summary>
        /// <param name="momentOfInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns>The elementary stiffness matrix.</returns>
        public Task<double[,]> CalculateElementStiffness(double momentOfInertia, double youngModulus, double elementLength)
        {
            double constant = momentOfInertia * youngModulus / Math.Pow(elementLength, 3);

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
        /// This method calculates the beam's stiffness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The structure stiffness matrix.</returns>
        public virtual async Task<double[,]> CalculateStiffnessAsync(TBeam beam, uint degreesOfFreedom)
        {
            double[,] stiffness = new double[degreesOfFreedom, degreesOfFreedom];

            for (uint n = 0; n < beam.NumberOfElements; n++)
            {
                double length = beam.Length / beam.NumberOfElements;
                double[,] elementStiffness = await this.CalculateElementStiffness(beam.GeometricProperty.MomentOfInertia[n], beam.Material.YoungModulus, length).ConfigureAwait(false);

                for (uint i = 2 * n; i < 2 * n + Constants.DegreesOfFreedomElement; i++)
                {
                    for (uint j = 2 * n; j < 2 * n + Constants.DegreesOfFreedomElement; j++)
                    {
                        stiffness[i, j] += elementStiffness[i - 2 * n, j - 2 * n];
                    }
                }
            }

            return stiffness;
        }

        /// <summary>
        /// This method calculates the beam's damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns>The structure damping matrix.</returns>
        public Task<double[,]> CalculateDamping(double[,] mass, double[,] stiffness)
        {
            int size = mass.GetLength(0);

            double[,] damping = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    damping[i, j] = Constants.Mi * mass[i, j] + Constants.Alpha * stiffness[i, j];
                }
            }

            return Task.FromResult(damping);
        }

        /// <summary>
        /// This method calculates the beam's force matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns>The structure force matrix.</returns>
        public virtual Task<double[]> CalculateForce(TBeam beam) => Task.FromResult(beam.Forces);

        /// <summary>
        /// This method builds the boundary condition matrix and the number of true boundary conditions.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>The boundary conditions matrix and the number of true boundary conditions.</returns>
        public virtual Task<(bool[], uint)> CalculateBoundaryConditionsAsync(TBeam beam, uint degreesOfFreedom)
        {
            bool[] boundaryConditions = new bool[degreesOfFreedom];

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                boundaryConditions[i] = true;
            }

            foreach (KeyValuePair<uint, FasteningType> fastening in beam.Fastenings)
            {
                boundaryConditions[2 * fastening.Key] = fastening.Value.AllowLinearDisplacement;
                boundaryConditions[2 * fastening.Key + 1] = fastening.Value.AllowAngularDisplacement;
            }

            uint numberOfTrueBoundaryConditions = 0;
            for (int i = 0; i < degreesOfFreedom; i++)
            {
                if (boundaryConditions[i] == true)
                {
                    numberOfTrueBoundaryConditions += 1;
                }
            }

            return Task.FromResult((boundaryConditions, numberOfTrueBoundaryConditions));
        }
    }
}
