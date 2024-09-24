﻿using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Mapper;
using System;

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
        public RungeKuttaForthOrderMethod(IDifferentialEquationOfMotion differentialEquationOfMotion, IMappingResolver mappingResolver)
            : base(mappingResolver)
        {
            this._differentialEquationOfMotion = differentialEquationOfMotion;
        }

        /// <summary>
        /// This delegate contains the method that calculates the differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public delegate double[] CalculateDifferentialEquationOfMotion<TInput>(TInput input, double time, double[] previousResult)
            where TInput : RigidBodyInput;

        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public double[] CalculateResult<TInput>(CalculateDifferentialEquationOfMotion<TInput> calculateDifferentialEquationOfMotion, TInput input, double time, double[] previousResult)
            where TInput : RigidBodyInput
        {
            int arrayLength = previousResult.Length;

            double[] result = new double[arrayLength];
            double[] t1 = new double[arrayLength];
            double[] t2 = new double[arrayLength];
            double[] t3 = new double[arrayLength];

            double[] y1 = calculateDifferentialEquationOfMotion(input, time, previousResult);
            for (int i = 0; i < arrayLength; i++)
            {
                t1[i] = previousResult[i] + 0.5 * input.TimeStep * y1[i];
            }

            double[] y2 = calculateDifferentialEquationOfMotion(input, time + input.TimeStep / 2, t1);
            for (int i = 0; i < arrayLength; i++)
            {
                t2[i] = previousResult[i] + 0.5 * input.TimeStep * y2[i];
            }

            double[] y3 = calculateDifferentialEquationOfMotion(input, time + input.TimeStep / 2, t2);
            for (int i = 0; i < arrayLength; i++)
            {
                t3[i] = previousResult[i] + input.TimeStep * y3[i];
            }

            double[] y4 = calculateDifferentialEquationOfMotion(input, time + input.TimeStep, t3);

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
        public override FiniteElementResult CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input)
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
        public override FiniteElementResult CalculateFiniteElementResult(FiniteElementMethodInput input,
            FiniteElementResult previousResult, double time)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override double[] CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult)
        {
            return this.CalculateResult((input1, time1, previousResult1) => this._differentialEquationOfMotion.CalculateForOneDegreeOfFreedom(input1, time1, previousResult1), input, time, previousResult);
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis using Runge Kutta Forth Order numerical method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override double[] CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time, double[] previousResult)
        {
            return this.CalculateResult((input1, time1, previousResult1) => this._differentialEquationOfMotion.CalculateForTwoDegreedOfFreedom(input1, time1, previousResult1), input, time, previousResult);
        }
    }
}
