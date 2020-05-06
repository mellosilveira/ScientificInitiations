using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.TimeOperation
{
    /// <summary>
    /// It contains operations evolving the time for the analysis.
    /// </summary>
    public class Time : ITime
    {
        /// <summary>
        /// Calculates the time step for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodDivision"></param>
        /// <returns></returns>
        public Task<double> CalculateTimeStep(double angularFrequency, uint periodDivision)
        {
            if (angularFrequency == 0)
            {
                double stepTime = 2 * Math.PI / periodDivision;

                return Task.FromResult(stepTime);
            }
            else
            {
                double period = 2 * Math.PI / angularFrequency;
                double stepTime = period / periodDivision;

                return Task.FromResult(stepTime);
            }
        }

        /// <summary>
        /// Calculates the final time for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodCount"></param>
        /// <returns></returns>
        public Task<double> CalculateFinalTime(double angularFrequency, uint periodCount)
        {
            if (angularFrequency == 0)
            {
                double finalTime = 2 * Math.PI * periodCount;

                return Task.FromResult(finalTime);
            }
            else
            {
                double period = 2 * Math.PI / angularFrequency;
                double finalTime = period * periodCount;

                return Task.FromResult(finalTime);
            }
        }
    }
}
