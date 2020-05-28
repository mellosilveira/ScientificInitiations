using IcVibracoes.Core.Models.BeamCharacteristics;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.Force
{
    /// <summary>
    /// It contains additionals operations evolving force.
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
        public Task<double> CalculateForceByType(double originalForce, double angularFrequency, double time, ForceType forceType)
        {
            double force = 0;

            if (forceType == ForceType.Harmonic)
            {
                force = originalForce * Math.Sin(angularFrequency * time);
            }
            else if (forceType == ForceType.Impact)
            {
                if (time == 0)
                {
                    force = originalForce;
                }
                else
                {
                    force = 0;
                }
            }

            return Task.FromResult(force);
        }
    }
}
