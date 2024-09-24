﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeOfFreedom;
using IcVibracoes.Core.Validators.MechanicalProperties;
using IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom;

namespace IcVibracoes.Core.Operations.CalculateVibration.RigidBody.OneDegreeOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibrationRigidBody<OneDegreeOfFreedomRequest, OneDegreeOfFreedomResponse, OneDegreeOfFreedomResponseData, OneDegreeOfFreedomInput>, ICalculateVibrationToOneDegreeFreedom
    {
        private static readonly string TemplateBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Solutions\\RigidBody\\OneDegreeOfFreedom");

        private readonly IMechanicalPropertiesValidator _mechanicalPropertiesValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mechanicalPropertiesValidator"></param>
        /// <param name="time"></param>
        public CalculateVibrationToOneDegreeFreedom(
            IMechanicalPropertiesValidator mechanicalPropertiesValidator,
            ITime time) : base(time)
        {
            this._mechanicalPropertiesValidator = mechanicalPropertiesValidator;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override double[] CalculateRigidBodyResult(OneDegreeOfFreedomInput input, double time, double[] previousResult)
            => base.NumericalMethod.CalculateOneDegreeOfFreedomResult(input, time, previousResult);

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override double[] BuildInitialConditions(OneDegreeOfFreedomRequest request)
        {
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod);
            if (numericalMethod == Models.NumericalMethod.RungeKuttaForthOrder)
            {
                // For Runge Kutta Forth Order numerical method, is used only 2 varibles.
                return new double[2];
            }

            return new double[Constants.NumberOfRigidBodyVariables1Df];
        }

        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="OneDegreeOfFreedomInput"/>.</returns>
        public override async Task<OneDegreeOfFreedomInput> CreateInputAsync(OneDegreeOfFreedomRequest request)
        {
            OneDegreeOfFreedomInput input = await base.CreateInputAsync(request).ConfigureAwait(false);
            input.Mass = request.ElementData.Mass;
            input.Stiffness = request.ElementData.Stiffness;
            input.DampingRatio = request.DampingRatios.FirstOrDefault();
            input.Force = request.Force;

            return input;
        }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public override string CreateSolutionPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string fileUri = Path.Combine(
                TemplateBasePath,
                $"m={input.Mass}_k={input.Stiffness}\\{input.ForceType}\\DampingRatio={input.DampingRatio}");

            string fileName = null;
            if (input.ForceType == ForceType.Harmonic)
            {
                fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}.csv";
            }
            else if (input.ForceType == ForceType.Impact)
            {
                fileName = $"{request.AnalysisType}.csv";
            }

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return path;
        }

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public override string CreateMaxValuesPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string fileUri = Path.Combine(
                TemplateBasePath,
                $"m={input.Mass}_k={input.Stiffness}\\{input.ForceType}",
                "MaxValues");

            string fileName = null;
            if (input.ForceType == ForceType.Harmonic)
            {
                fileName = $"MaxValues_{request.AnalysisType}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_dampingRatio={input.DampingRatio}.csv";
            }
            else if (input.ForceType == ForceType.Impact)
            {
                fileName = $"MaxValues_{request.AnalysisType}_dampingRatio={input.DampingRatio}.csv";
            }

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return path;
        }

        /// <summary>
        /// This method validates the <see cref="OneDegreeOfFreedomRequest"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<OneDegreeOfFreedomResponse> ValidateOperationAsync(OneDegreeOfFreedomRequest request)
        {
            OneDegreeOfFreedomResponse response = await base.ValidateOperationAsync(request).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.ElementData, response).ConfigureAwait(false);

            return response;
        }
    }
}