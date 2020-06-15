using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF;
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
    public class CalculateVibrationToTwoDegreesFreedom : CalculateVibration_RigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput>, ICalculateVibrationToTwoDegreesFreedom
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="numericalMethod"></param>
        /// <param name="file"></param>
        /// <param name="time"></param>
        public CalculateVibrationToTwoDegreesFreedom(
            IRungeKuttaForthOrderMethod_2DF numericalMethod, 
            IFile file, 
            ITime time) 
            : base(numericalMethod, file, time)
        {
        }

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(TwoDegreesOfFreedomRequest request)
        {
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_2DF]
            {
                request.PrimaryElementData.InitialDisplacement,
                request.PrimaryElementData.InitialVelocity,
                request.SecondaryElementData.InitialDisplacement,
                request.SecondaryElementData.InitialVelocity
            });
        }        

        /// <summary>
        /// Creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<TwoDegreesOfFreedomInput> CreateInput(TwoDegreesOfFreedomRequest request)
        {
            if (request == null || request.PrimaryElementData == null || request.SecondaryElementData== null)
            {
                return null;
            }

            return Task.FromResult(new TwoDegreesOfFreedomInput
            {
                AngularFrequency = request.InitialAngularFrequency,
                AngularFrequencyStep = request.AngularFrequencyStep,
                FinalAngularFrequency = request.FinalAngularFrequency,
                DampingRatio = request.DampingRatioList.FirstOrDefault(),
                Force = request.Force,
                ForceType = ForceTypeFactory.Create(request.ForceType),
                Stiffness = request.PrimaryElementData.MechanicalProperties.Stiffness,
                Mass = request.PrimaryElementData.MechanicalProperties.Mass,
                SecondaryStiffness = request.SecondaryElementData.MechanicalProperties.Stiffness,
                SecondaryMass = request.SecondaryElementData.MechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Task<string> CreateSolutionPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}.csv";

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/m1={input.Mass}_k1={input.Stiffness}/m2={input.SecondaryMass}_k2={input.SecondaryStiffness}/DampingRatio={input.DampingRatio}");

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
        public override Task<string> CreateMaxValuesPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());
            
            string fileName = $"MaxValues_{request.AnalysisType}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}.csv";

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/OneDegreeFreedom/m1={input.Mass}_k1={input.Stiffness}/m2={input.SecondaryMass}_k2={input.SecondaryStiffness}/DampingRatio={input.DampingRatio}",
                "MaxValues");

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);
       
            return Task.FromResult(path);
        }
    }
}
