using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom;
using IcVibracoes.Core.Validators.MechanicalProperties;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom;

namespace IcVibracoes.Core.Operations.CalculateVibration.RigidBody.TwoDegreesOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom.
    /// </summary>
    public class CalculateVibrationToTwoDegreesOfFreedom : CalculateVibrationRigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput>, ICalculateVibrationToTwoDegreesOfFreedom
    {
        private readonly IMechanicalPropertiesValidator _mechanicalPropertiesValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mechanicalPropertiesValidator"></param>
        /// <param name="time"></param>
        public CalculateVibrationToTwoDegreesOfFreedom(
            IMechanicalPropertiesValidator mechanicalPropertiesValidator,
            ITime time) : base(time)
        {
            this._mechanicalPropertiesValidator = mechanicalPropertiesValidator;
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override double[] CalculateRigidBodyResult(TwoDegreesOfFreedomInput input, double time, double[] previousResult)
            => base.NumericalMethod.CalculateTwoDegreesOfFreedomResult(input, time, previousResult);

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override double[] BuildInitialConditions(TwoDegreesOfFreedomRequest request)
        {
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod);
            if (numericalMethod == Models.NumericalMethod.RungeKuttaForthOrder)
            {
                // For Runge Kutta Forth Order numerical method, is used only 4 variables.
                return new double[4];
            }

            return new double[Constants.NumberOfRigidBodyVariables2Df];
        }

        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="TwoDegreesOfFreedomInput"/>.</returns>
        public override TwoDegreesOfFreedomInput CreateInput(TwoDegreesOfFreedomRequest request)
        {
            TwoDegreesOfFreedomInput input = base.CreateInput(request);
            input.Mass = request.PrimaryElementData.Mass;
            input.SecondaryMass = request.SecondaryElementData.Mass;
            input.Stiffness = request.PrimaryElementData.Stiffness;
            input.SecondaryStiffness = request.SecondaryElementData.Stiffness;
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
        public override string CreateSolutionPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\TwoDegreesOfFreedom\\m1={input.Mass}_k1={input.Stiffness}\\m2={input.SecondaryMass}_k2={input.SecondaryStiffness}\\DampingRatio={input.DampingRatio}");

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
        public override string CreateMaxValuesPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\TwoDegreesOfFreedom\\m1={input.Mass}_k1={input.Stiffness}\\m2={input.SecondaryMass}_k2={input.SecondaryStiffness}",
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
        /// This method validates the <see cref="TwoDegreesOfFreedomRequest"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<TwoDegreesOfFreedomResponse> ValidateOperationAsync(TwoDegreesOfFreedomRequest request)
        {
            TwoDegreesOfFreedomResponse response = await base.ValidateOperationAsync(request).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.PrimaryElementData, response).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.SecondaryElementData, response).ConfigureAwait(false);

            return response;
        }
    }
}
