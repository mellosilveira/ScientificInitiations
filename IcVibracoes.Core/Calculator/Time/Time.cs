using IcVibracoes.Core.Calculator.NaturalFrequency;
using System;

namespace IcVibracoes.Core.Calculator.Time
{
    /// <summary>
    /// It contains operations evolving the time for the analysis.
    /// </summary>
    public class Time : ITime
    {
        private readonly INaturalFrequency _naturalFrequency;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="naturalFrequency"></param>
        public Time(INaturalFrequency naturalFrequency)
        {
            this._naturalFrequency = naturalFrequency;
        }

        /// <summary>
        /// Calculates the time step for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodDivision"></param>
        /// <returns></returns>
        public double CalculateTimeStep(double angularFrequency, uint periodDivision)
        {
            if (angularFrequency == 0)
            {
                return 2 * Math.PI / periodDivision;
            }

            double period = 2 * Math.PI / angularFrequency;
            double stepTime = period / periodDivision;

            return stepTime;
        }

        /// <summary>
        /// Calculates the final time for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodCount"></param>
        /// <returns></returns>
        public double CalculateFinalTime(double angularFrequency, uint periodCount)
        {
            if (angularFrequency == 0)
            {
                return 2 * Math.PI;
            }
            
            double period = 2 * Math.PI / angularFrequency;
            double finalTime = period * periodCount;

            return finalTime;
        }

        /// <summary>
        /// Calculates the time step for Runge Kutta Forth Order Method.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="periodDivision"></param>
        /// <returns></returns>
        public double CalculateTimeStep(double mass, double stiffness, double angularFrequency, uint periodDivision)
        {
            double naturalPeriod = this.CalculateNaturalPeriod(mass, stiffness);

            double period = 2 * Math.PI / angularFrequency;
            double timeStep = period / periodDivision;

            // Natural time is divided by 10, because it's the maximum value to time step accepted in Runge Kutta Forth Order Method.
            if (timeStep < naturalPeriod / 10 && angularFrequency != 0)
            {
                return timeStep;
            }

            return naturalPeriod / periodDivision;
        }

        /// <summary>
        /// Calculates the natural period for rigid body analysis.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public double CalculateNaturalPeriod(double mass, double stiffness)
        {
            double naturalFrequency = this._naturalFrequency.Calculate(mass, stiffness);

            double naturalPeriod = 2 * Math.PI / naturalFrequency;

            return naturalPeriod;
        }
    }
}
