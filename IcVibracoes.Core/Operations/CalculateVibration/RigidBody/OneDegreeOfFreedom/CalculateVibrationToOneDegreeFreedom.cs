using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_1DF;
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
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibration_RigidBody<OneDegreeOfFreedomRequest, OneDegreeOfFreedomRequestData, OneDegreeOfFreedomResponse, OneDegreeOfFreedomResponseData, OneDegreeOfFreedomInput>, ICalculateVibrationToOneDegreeFreedom
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="numericalMethod"></param>
        /// <param name="file"></param>
        /// <param name="time"></param>
        public CalculateVibrationToOneDegreeFreedom(
            IRungeKuttaForthOrderMethod_1DF numericalMethod, 
            IFile file, 
            ITime time) 
            : base(numericalMethod, file, time)
        {
        }

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(OneDegreeOfFreedomRequestData requestData)
        {
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_1DF]
            {
                requestData.InitialDisplacement,
                requestData.InitialVelocity
            });
        }

        /// <summary>
        /// Creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<OneDegreeOfFreedomInput> CreateInput(OneDegreeOfFreedomRequest request)
        {
            if (request.Data == null || request.Data.MechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new OneDegreeOfFreedomInput
            {
                AngularFrequency = request.Data.InitialAngularFrequency,
                AngularFrequencyStep = request.Data.AngularFrequencyStep,
                FinalAngularFrequency = request.Data.FinalAngularFrequency,
                DampingRatio = request.Data.DampingRatioList.FirstOrDefault(),
                Force = request.Data.Force,
                ForceType = ForceTypeFactory.Create(request.Data.ForceType),
                Stiffness = request.Data.MechanicalProperties.Stiffness,
                Mass = request.Data.MechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public override Task<string> CreateSolutionPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input, OneDegreeOfFreedomResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/m={input.Mass}_k={input.Stiffness}");

            string fileName = $"{request.AnalysisType.Trim()}_m={input.Mass}_k={input.Stiffness}_dampingRatio={input.DampingRatio}_w={Math.Round(input.AngularFrequency, 2)}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }

        /// <summary>
        /// Creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public override Task<string> CreateMaxValuesPath(OneDegreeOfFreedomRequest request, OneDegreeOfFreedomInput input, OneDegreeOfFreedomResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType.Trim()}_m={input.Mass}_k={input.Stiffness}_dampingRatio={input.DampingRatio}_w0={Math.Round(request.Data.InitialAngularFrequency, 2)}_wf={Math.Round(request.Data.FinalAngularFrequency)}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}