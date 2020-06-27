using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
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
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="time"></param>
        public CalculateVibrationToOneDegreeFreedom(
            IFile file,
            ITime time)
            : base(file, time)
        { }

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
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_1DF]
            {
                request.ElementData.InitialDisplacement,
                request.ElementData.InitialVelocity
            });
        }

        /// <summary>
        /// Creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<OneDegreeOfFreedomInput> CreateInput(OneDegreeOfFreedomRequest request)
        {
            if (request == null || request.ElementData == null)
            {
                return null;
            }

            return Task.FromResult(new OneDegreeOfFreedomInput
            {
                AngularFrequency = request.InitialAngularFrequency,
                AngularFrequencyStep = request.AngularFrequencyStep,
                FinalAngularFrequency = request.FinalAngularFrequency,
                DampingRatio = request.DampingRatioList.FirstOrDefault(),
                Force = request.Force,
                ForceType = (ForceType)Enum.Parse(typeof(ForceType), request.ForceType, ignoreCase: true),
                Stiffness = request.ElementData.MechanicalProperties.Stiffness,
                Mass = request.ElementData.MechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Task<string> CreateSolutionPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/m={input.Mass}_k={input.Stiffness}/{input.ForceType}/DampingRatio={input.DampingRatio}");

            string fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        /// <summary>
        /// Creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Task<string> CreateMaxValuesPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/m={input.Mass}_k={input.Stiffness}/{input.ForceType}/DampingRatio={input.DampingRatio}",
                "MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }
    }
}