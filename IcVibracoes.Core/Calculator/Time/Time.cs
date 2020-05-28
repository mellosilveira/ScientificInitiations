using IcVibracoes.Core.Calculator.NaturalFrequency;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.Time
{
    /// <summary>
    /// It contains operations evolving the time for the analysis.
    /// </summary>
    public class Time : ITime
    {
        private readonly INaturalFrequency _naturalFrequency;

        public Time(INaturalFrequency naturalFrequency)
        {
            _naturalFrequency = naturalFrequency;
        }

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
                double finalTime = 2 * Math.PI;

                return Task.FromResult(finalTime);
            }
            else
            {
                double period = 2 * Math.PI / angularFrequency;
                double finalTime = period * periodCount;

                return Task.FromResult(finalTime);
            }
        }

        /// <summary>
        /// Calculates the time step for Runge Kutta Forth Order Method.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="periodDivision"></param>
        /// <returns></returns>
        public async Task<double> CalculateTimeStep(double mass, double stiffness, double angularFrequency, uint periodDivision)
        {
            double naturalPeriod = await CalculateNaturalPeriod(mass, stiffness).ConfigureAwait(false);

            double period = 2 * Math.PI / angularFrequency;
            double timeStep = period / periodDivision;

            // Natural time is divided by 10, because it's the maximum value to time step accepted in Runge Kutta Forth Order Method.
            if (timeStep < naturalPeriod / 10)
            {
                return timeStep;
            }
            else
            {
                return naturalPeriod / periodDivision;
            }
        }

        /// <summary>
        /// Calculates the natural period for rigid body analysis.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public async Task<double> CalculateNaturalPeriod(double mass, double stiffness)
        {
            double naturalFrequency = await _naturalFrequency.Calculate(mass, stiffness).ConfigureAwait(false);

            double naturalPeriod = 2 * Math.PI / naturalFrequency;

            return naturalPeriod;
        }
    }
}
