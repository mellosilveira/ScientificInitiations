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
    public class CalculateVibrationToTwoDegreesFreedom : CalculateVibration_RigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomRequestData, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput>, ICalculateVibrationToTwoDegreesFreedom
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
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(TwoDegreesOfFreedomRequestData requestData)
        {
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_2DF]
            {
                requestData.PrimaryInitialDisplacement,
                requestData.PrimaryInitialVelocity,
                requestData.SecondaryInitialDisplacement,
                requestData.SecondaryInitialVelocity
            });
        }        

        /// <summary>
        /// Creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<TwoDegreesOfFreedomInput> CreateInput(TwoDegreesOfFreedomRequest request)
        {
            if (request.Data == null || request.Data.MainObjectMechanicalProperties == null || request.Data.SecondaryObjectMechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new TwoDegreesOfFreedomInput
            {
                AngularFrequency = request.Data.InitialAngularFrequency,
                AngularFrequencyStep = request.Data.AngularFrequencyStep,
                FinalAngularFrequency = request.Data.FinalAngularFrequency,
                DampingRatio = request.Data.DampingRatioList.FirstOrDefault(),
                Force = request.Data.Force,
                ForceType = ForceTypeFactory.Create(request.Data.ForceType),
                Stiffness = request.Data.MainObjectMechanicalProperties.Stiffness,
                Mass = request.Data.MainObjectMechanicalProperties.Mass,
                SecondaryStiffness = request.Data.SecondaryObjectMechanicalProperties.Stiffness,
                SecondaryMass = request.Data.SecondaryObjectMechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public override Task<string> CreateSolutionPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input, TwoDegreesOfFreedomResponse response)
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
        /// <param name="response"></param>
        /// <returns></returns>
        public override Task<string> CreateMaxValuesPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input, TwoDegreesOfFreedomResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());
            
            string fileName = $"MaxValues_{request.AnalysisType}_w0={Math.Round(request.Data.InitialAngularFrequency, 2)}_wf={Math.Round(request.Data.FinalAngularFrequency, 2)}.csv";

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
