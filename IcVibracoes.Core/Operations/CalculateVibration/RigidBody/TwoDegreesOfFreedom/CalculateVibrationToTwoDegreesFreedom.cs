using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Validators.MechanicalProperties;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom.
    /// </summary>
    public class CalculateVibrationToTwoDegreesOfFreedom : CalculateVibration_RigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput>, ICalculateVibrationToTwoDegreesOfFreedom
    {
        private readonly IMechanicalPropertiesValidator _mechanicalPropertiesValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mechanicalPropertiesValidator"></param>
        /// <param name="time"></param>
        public CalculateVibrationToTwoDegreesOfFreedom(
            IMechanicalPropertiesValidator mechanicalPropertiesValidator,
            ITime time)
            : base(time)
        {
            this._mechanicalPropertiesValidator = mechanicalPropertiesValidator;
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateRigidBodyResult(TwoDegreesOfFreedomInput input, double time, double[] y)
            => base._numericalMethod.CalculateTwoDegreesOfFreedomResult(input, time, y);

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(TwoDegreesOfFreedomRequest request)
        {
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod);
            if (numericalMethod == NumericalMethod.RungeKuttaForthOrder)
            {
                // For Runge Kutta Forth Order numerical method, is used only 4 varibles.
                return Task.FromResult(new double[4]);
            }

            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_2DF]);
        }

        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="TwoDegreesOfFreedomInput"/>.</returns>
        public override async Task<TwoDegreesOfFreedomInput> CreateInput(TwoDegreesOfFreedomRequest request)
        {
            TwoDegreesOfFreedomInput input = await base.CreateInput(request).ConfigureAwait(false);
            input.Mass = request.PrimaryElementData.Mass;
            input.SecondaryMass = request.SecondaryElementData.Mass;
            input.Stiffness = request.PrimaryElementData.Stiffness;
            input.SecondaryStiffness = request.SecondaryElementData.Stiffness;
            input.DampingRatio = request.DampingRatios.FirstOrDefault();
            input.Force = request.Force;
            input.ForceType = (ForceType)Enum.Parse(typeof(ForceType), request.ForceType, ignoreCase: true);

            return input;
        }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public override Task<string> CreateSolutionPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\TwoDegreesOfFreedom\\m1={input.Mass}_k1={input.Stiffness}\\m2={input.SecondaryMass}_k2={input.SecondaryStiffness}\\DampingRatio={input.DampingRatio}");

            string fileName = null;
            if (input.ForceType == ForceType.Harmonic)
            {
                fileName = $"{request.AnalysisType}_w={input.AngularFrequency}.csv";
            }
            else if (input.ForceType == ForceType.Impact)
            {
                fileName = $"{request.AnalysisType}.csv";
            }

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public override Task<string> CreateMaxValuesPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\TwoDegreesOfFreedom\\m1={input.Mass}_k1={input.Stiffness}\\m2={input.SecondaryMass}_k2={input.SecondaryStiffness}",
                "MaxValues");

            string fileName = null;
            if (input.ForceType == ForceType.Harmonic)
            {
                fileName = $"MaxValues_{request.AnalysisType}_w0={request.InitialAngularFrequency}_wf={request.FinalAngularFrequency}_dampingRatio={input.DampingRatio}.csv";
            }
            else if (input.ForceType == ForceType.Impact)
            {
                fileName = $"MaxValues_{request.AnalysisType}_dampingRatio={input.DampingRatio}.csv";
            }

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        /// <summary>
        /// This method validates the <see cref="TwoDegreesOfFreedomRequest"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<TwoDegreesOfFreedomResponse> ValidateOperation(TwoDegreesOfFreedomRequest request)
        {
            TwoDegreesOfFreedomResponse response = await base.ValidateOperation(request).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.PrimaryElementData, response).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.SecondaryElementData, response).ConfigureAwait(false);

            return response;
        }
    }
}
