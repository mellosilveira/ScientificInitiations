﻿using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public class RungeKuttaForthOrderMethod : NumericalIntegrationMethod, IRungeKuttaForthOrderMethod
    {
        private readonly IDifferentialEquationOfMotion _differentialEquationOfMotion;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="differentialEquationOfMotion"></param>
        public RungeKuttaForthOrderMethod(IDifferentialEquationOfMotion differentialEquationOfMotion)
        {
            this._differentialEquationOfMotion = differentialEquationOfMotion;
        }

        /// <summary>
        /// This delegate contains the method that calculates the differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public delegate Task<double[]> CalculateDifferentialEquationOfMotion<TInput>(TInput input, double time, double[] y)
            where TInput : RigidBodyInput;

        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateResult<TInput>(CalculateDifferentialEquationOfMotion<TInput> calculateDifferentialEquationOfMotion, TInput input, double time, double[] y)
            where TInput : RigidBodyInput
        {
            int arrayLength = y.Length;

            double[] result = new double[arrayLength];
            double[] t1 = new double[arrayLength];
            double[] t2 = new double[arrayLength];
            double[] t3 = new double[arrayLength];

            double[] y1 = await calculateDifferentialEquationOfMotion(input, time, y).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t1[i] = y[i] + 0.5 * input.TimeStep * y1[i];
            }

            double[] y2 = await calculateDifferentialEquationOfMotion(input, time + input.TimeStep / 2, t1).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t2[i] = y[i] + 0.5 * input.TimeStep * y2[i];
            }

            double[] y3 = await calculateDifferentialEquationOfMotion(input, time + input.TimeStep / 2, t2).ConfigureAwait(false);
            for (int i = 0; i < arrayLength; i++)
            {
                t3[i] = y[i] + input.TimeStep * y3[i];
            }

            double[] y4 = await calculateDifferentialEquationOfMotion(input, time + input.TimeStep, t3).ConfigureAwait(false);

            for (int i = 0; i < arrayLength; i++)
            {
                result[i] = (y1[i] + 2 * y2[i] + 2 * y3[i] + y4[i]) * (input.TimeStep / 6);
            }

            return result;
        }

        /// <summary>
        /// Calculates the result for the initial time for a finite element analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Task<FiniteElementResult> CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Task<FiniteElementResult> CalculateFiniteElementResult(FiniteElementMethodInput input, FiniteElementResult previousResult, double time)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] y)
        {
            return await this.CalculateResult(this._differentialEquationOfMotion.CalculateForOneDegreeOfFreedom, input, time, y).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time, double[] y)
        {
            return await this.CalculateResult(this._differentialEquationOfMotion.CalculateForTwoDegreedOfFreedom, input, time, y).ConfigureAwait(false);
        }
    }
}
