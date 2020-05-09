﻿using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.ForceOperation
{
    /// <summary>
    /// It contains additionals operations evolving force.
    /// </summary>
    public interface IForceOperation
    {
        /// <summary>
        /// Calculates the force for a aspecific time based on its type.
        /// </summary>
        /// <param name="originalForce"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="time"></param>
        /// <param name="forceType"></param>
        /// <returns></returns>
        Task<double> CalculateForceByType(double originalForce, double angularFrequency, double time, ForceType forceType);
    }
}
