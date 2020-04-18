﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.MainMatrixes.Beam
{
    /// <summary>
    /// It's responsible to calculate the beam main matrixes.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public interface IBeamMainMatrix<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// It's responsible to calculate the element mass matrix.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="specificMass"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementMass(double area, double specificMass, double elementLength);

        /// <summary>
        /// Responsible to calculate the beam mass matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateMass(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the beam element hardness matrix.
        /// </summary>
        /// <param name="momentInertia"></param>
        /// <param name="youngModulus"></param>
        /// <param name="elementLength"></param>
        /// <returns></returns>
        Task<double[,]> CalculateElementHardness(double momentInertia, double youngModulus, double elementLength);

        /// <summary>
        /// Responsible to calculate the beam hardness matrix.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[,]> CalculateHardness(Beam<TProfile> beam, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the damping matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="hardness"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<double[,]> CalculateDamping(double[,] mass, double[,] hardness);

        /// <summary>
        /// It's rewsponsible to build the bondary condition matrix.
        /// </summary>
        /// <param name="firstFastening"></param>
        /// <param name="lastFastening"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<bool[]> CalculateBondaryCondition(Fastening firstFastening, Fastening lastFastening, uint degreesFreedomMaximum);
    }
}