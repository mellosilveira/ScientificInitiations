using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.NaturalFrequency;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public class CalculateVibrationToTwoDegreesFreedom : CalculateVibration_RigidBody<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, ICalculateVibrationToTwoDegreesFreedom
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibrationToTwoDegreesFreedom(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod_2DF rungeKutta)
            : base(auxiliarOperation, rungeKutta)
        { }

        /// <summary>
        /// Builds the input of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<DifferentialEquationOfMotionInput> BuildDifferentialEquationOfMotionInput(TwoDegreesFreedomRequestData requestData)
        {
            if (requestData == null || requestData.MainObjectMechanicalProperties == null || requestData.SecondaryObjectMechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new DifferentialEquationOfMotionInput
            {
                AngularFrequency = requestData.AngularFrequencyStep,
                DampingRatio = requestData.DampingRatioList.FirstOrDefault(),
                Force = requestData.Force,
                Stiffness = requestData.MainObjectMechanicalProperties.Stiffness,
                Mass = requestData.MainObjectMechanicalProperties.Mass,
                SecondaryStiffness = requestData.SecondaryObjectMechanicalProperties.Stiffness,
                SecondaryMass = requestData.SecondaryObjectMechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(TwoDegreesFreedomRequestData requestData)
        {
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_2DF]
            {
                requestData.PrimaryInitialDisplacement,
                requestData.PrimaryInitialVelocity,
                requestData.SecondaryInitialDisplacement,
                requestData.SecondaryInitialVelocity
            });
        }        

        public override Task<string> CreateSolutionPath(TwoDegreesFreedomResponse response, TwoDegreesFreedomRequestData requestData, string analysisType, double dampingRatio, double angularFrequency)
        {
            string path = Directory.GetCurrentDirectory();

            string folderName = Path.GetFileName(path);

            string previousPath = path.Substring(0, path.Length - folderName.Length);

            path = Path.Combine(
                previousPath,
                "Solutions/RigidBody/TwoDegreesFreedom",
                $"{analysisType.Trim()}_m1={requestData.MainObjectMechanicalProperties.Mass}_k1={requestData.MainObjectMechanicalProperties.Stiffness}_m2={requestData.SecondaryObjectMechanicalProperties.Mass}_k2={requestData.SecondaryObjectMechanicalProperties.Stiffness}_w={Math.Round(angularFrequency, 2)}_dampingRatio={dampingRatio}.csv");

            if (File.Exists(path))
            {
                response.AddError(ErrorCode.OperationError, $"File already exist in path '{path}'.");
                return null;
            }

            return Task.FromResult(path);
        }
    }
}
