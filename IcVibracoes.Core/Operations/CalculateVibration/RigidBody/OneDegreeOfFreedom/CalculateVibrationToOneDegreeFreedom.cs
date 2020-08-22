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
            OneDegreeOfFreedomInput initialInput = await base.CreateInput(request).ConfigureAwait(false);

            return new OneDegreeOfFreedomInput
            {
                AngularFrequency = initialInput.AngularFrequency,
                AngularFrequencyStep = initialInput.AngularFrequencyStep,
                FinalAngularFrequency = initialInput.FinalAngularFrequency,
                DampingRatio = request.DampingRatios.FirstOrDefault(),
                Force = request.Force,
                ForceType = (ForceType)Enum.Parse(typeof(ForceType), request.ForceType, ignoreCase: true),
                Stiffness = request.ElementData.Stiffness,
                Mass = request.ElementData.Mass
            };
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
                $"Solutions/RigidBody/OneDegreeOfFreedom/m={input.Mass}_k={input.Stiffness}/{input.ForceType}/DampingRatio={input.DampingRatio}");

            string fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}.csv";

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
                $"Solutions/RigidBody/OneDegreeOFFreedom/m={input.Mass}_k={input.Stiffness}/{input.ForceType}",
                "MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_dampingRatio={input.DampingRatio}.csv";

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