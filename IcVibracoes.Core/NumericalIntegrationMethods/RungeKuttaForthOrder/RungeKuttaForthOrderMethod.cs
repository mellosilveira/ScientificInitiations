﻿using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public abstract class RungeKuttaForthOrderMethod<TInput> : IRungeKuttaForthOrderMethod<TInput>
        where TInput : RigidBodyInput
    {
        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// For each case, with one or two degrees of freedom, there is a different differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract Task<double[]> CalculateDifferencialEquationOfMotion(TInput input, double time, double[] y);

        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateResult(TInput input, double time, double[] y)
        {
            int arrayLength = y.Length;

            double[] result = new double[arrayLength];
            double[] t1 = new double[arrayLength];
            double[] t2 = new double[arrayLength];
            double[] t3 = new double[arrayLength];

            double[] y1 = await this.CalculateDifferencialEquationOfMotion(input, time, y).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t1[i] = y[i] + 0.5 * input.TimeStep * y1[i];
            }

            double[] y2 = await this.CalculateDifferencialEquationOfMotion(input, time + input.TimeStep / 2, t1).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t2[i] = y[i] + 0.5 * input.TimeStep * y2[i];
            }

            double[] y3 = await this.CalculateDifferencialEquationOfMotion(input, time + input.TimeStep / 2, t2).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t3[i] = y[i] + input.TimeStep * y3[i];
            }

            double[] y4 = await this.CalculateDifferencialEquationOfMotion(input, time + input.TimeStep, t3).ConfigureAwait(false);

            for (int i = 0; i < arrayLength; i++)
            {
                result[i] = (y1[i] + 2 * y2[i] + 2 * y3[i] + y4[i]) * (input.TimeStep / 6);
            }

            return result;
        }
    }
}
