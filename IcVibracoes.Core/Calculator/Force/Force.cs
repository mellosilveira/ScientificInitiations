using IcVibracoes.Core.Models.BeamCharacteristics;
using System;

namespace IcVibracoes.Core.Calculator.Force
{
    /// <summary>
    /// It contains additional operations evolving force.
    /// </summary>
    public class Force : IForce
    {
        /// <summary>
        /// Calculates the force for a aspecific time based on its type.
        /// </summary>
        /// <param name="originalForce"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="time"></param>
        /// <param name="forceType"></param>
        /// <returns></returns>
        public double CalculateForceByType(double originalForce, double angularFrequency, double time,
            ForceType forceType)
        {
            var force = forceType switch
            {
                ForceType.Harmonic => (originalForce * Math.Sin(angularFrequency * time)),
                ForceType.Impact => ((time == 0) ? originalForce : 0),
                _ => 0
            };

            return force;
        }

        /// <summary>
        /// Calculates the force for a specific time based on its type.
        /// </summary>
        /// <param name="originalForce"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="time"></param>
        /// <param name="forceType"></param>
        /// <returns></returns>
        public double[] CalculateForceByType(double[] originalForce, double angularFrequency, double time, ForceType forceType)
        {
            double[] force = new double[originalForce.Length];

            for (int i = 0; i < originalForce.Length; i++)
            {
                force[i] = this.CalculateForceByType(originalForce[i], angularFrequency, time, forceType);
            }

            return force;
        }
    }
}
