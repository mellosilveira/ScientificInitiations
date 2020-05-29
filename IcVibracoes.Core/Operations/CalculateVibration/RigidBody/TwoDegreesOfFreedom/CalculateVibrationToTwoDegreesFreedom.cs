using IcVibracoes.Core.AuxiliarOperations;
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
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public class CalculateVibrationToTwoDegreesFreedom : CalculateVibration_RigidBody<TwoDegreesOfFreedomRequest, TwoDegreesOfFreedomRequestData, TwoDegreesOfFreedomResponse, TwoDegreesOfFreedomResponseData, TwoDegreesOfFreedomInput>, ICalculateVibrationToTwoDegreesFreedom
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        /// <param name="time"></param>
        public CalculateVibrationToTwoDegreesFreedom(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod_2DF rungeKutta,
            ITime time)
            : base(auxiliarOperation, rungeKutta, time)
        { }

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

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/TwoDegreesOfFreedom/m1={input.Mass}_k1={input.Stiffness}/m2={input.SecondaryMass}_k2={input.SecondaryStiffness}");

            string fileName = $"{request.AnalysisType.Trim()}_m1={input.Mass}_k1={input.Stiffness}_m2={input.SecondaryMass}_k2={input.SecondaryStiffness}_w={Math.Round(input.AngularFrequency, 2)}_dampingRatio={input.DampingRatio}.csv";

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
        public override Task<string> CreateMaxValuesPath(TwoDegreesOfFreedomRequest request, TwoDegreesOfFreedomInput input, TwoDegreesOfFreedomResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/RigidBody/TwoDegreesOfFreedom/MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType.Trim()}_m1={input.Mass}_k1={input.Stiffness}_m2={input.SecondaryMass}_k2={input.SecondaryStiffness}_dampingRatio={input.DampingRatio}_w0={Math.Round(request.Data.InitialAngularFrequency, 2)}_wf={Math.Round(request.Data.FinalAngularFrequency)}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}
