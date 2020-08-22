using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Validators.MechanicalProperties;
using IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeOfFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibration_RigidBody<OneDegreeOfFreedomRequest, OneDegreeOfFreedomResponse, OneDegreeOfFreedomResponseData, OneDegreeOfFreedomInput>, ICalculateVibrationToOneDegreeFreedom
    {
        private readonly IMechanicalPropertiesValidator _mechanicalPropertiesValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mechanicalPropertiesValidator"></param>
        /// <param name="time"></param>
        public CalculateVibrationToOneDegreeFreedom(
            IMechanicalPropertiesValidator mechanicalPropertiesValidator,
            ITime time)
            : base(time)
        {
            this._mechanicalPropertiesValidator = mechanicalPropertiesValidator;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateRigidBodyResult(OneDegreeOfFreedomInput input, double time, double[] y)
            => base._numericalMethod.CalculateOneDegreeOfFreedomResult(input, time, y);

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(OneDegreeOfFreedomRequest request)
        {
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod);
            if (numericalMethod == NumericalMethod.RungeKuttaForthOrder)
            {
                // For Runge Kutta Forth Order numerical method, is used only 2 varibles.
                return Task.FromResult(new double[2]);
            }

            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_1DF]);
        }

        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="OneDegreeOfFreedomInput"/>.</returns>
        public override async Task<OneDegreeOfFreedomInput> CreateInput(OneDegreeOfFreedomRequest request)
        {
            OneDegreeOfFreedomInput input = await base.CreateInput(request).ConfigureAwait(false);
            input.Mass = request.ElementData.Mass;
            input.Stiffness = request.ElementData.Stiffness;
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
        public override Task<string> CreateSolutionPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\OneDegreeOfFreedom\\m={input.Mass}_k={input.Stiffness}\\{input.ForceType}\\DampingRatio={input.DampingRatio}");

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
        public override Task<string> CreateMaxValuesPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions\\RigidBody\\OneDegreeOFFreedom\\m={input.Mass}_k={input.Stiffness}\\{input.ForceType}",
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
        /// This method validates the <see cref="OneDegreeOfFreedomRequest"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<OneDegreeOfFreedomResponse> ValidateOperation(OneDegreeOfFreedomRequest request)
        {
            OneDegreeOfFreedomResponse response = await base.ValidateOperation(request).ConfigureAwait(false);

            await this._mechanicalPropertiesValidator.Execute(request.ElementData, response).ConfigureAwait(false);

            return response;
        }
    }
}