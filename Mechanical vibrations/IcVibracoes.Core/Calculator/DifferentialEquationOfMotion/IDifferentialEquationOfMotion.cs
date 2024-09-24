﻿using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;

namespace IcVibracoes.Core.Calculator.DifferentialEquationOfMotion
{
    /// <summary>
    /// It's responsible to calculate the differential equation of motion.
    /// </summary>
    public interface IDifferentialEquationOfMotion
    {
        /// <summary>
        /// Calculates the value of the differential equation of motion used for the one degree of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        double[] CalculateForOneDegreeOfFreedom(OneDegreeOfFreedomInput input, double time, double[] previousResult);

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the two degrees of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        double[] CalculateForTwoDegreedOfFreedom(TwoDegreesOfFreedomInput input, double time, double[] previousResult);
    }
}