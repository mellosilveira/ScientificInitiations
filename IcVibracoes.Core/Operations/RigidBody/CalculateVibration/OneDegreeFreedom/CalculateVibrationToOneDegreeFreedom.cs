using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibration_RigidBody<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, ICalculateVibrationToOneDegreeFreedom
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibrationToOneDegreeFreedom(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod_1DF rungeKutta) : base(auxiliarOperation, rungeKutta)
        { }

        /// <summary>
        /// Builds the input of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<DifferentialEquationOfMotionInput> BuildDifferentialEquationOfMotionInput(OneDegreeFreedomRequestData requestData)
        {
            if (requestData == null || requestData.MechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new DifferentialEquationOfMotionInput
            {
                AngularFrequency = requestData.AndularFrequencyStep,
                DampingRatio = requestData.DampingRatioList.FirstOrDefault(),
                Force = requestData.Force,
                Stiffness = requestData.MechanicalProperties.Stiffness,
                Mass = requestData.MechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public override Task<double[]> BuildInitialConditions(OneDegreeFreedomRequestData requestData)
        {
            return Task.FromResult(new double[Constant.NumberOfRigidBodyVariables_1DF]
            {
                requestData.InitialDisplacement,
                requestData.InitialVelocity
            });
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];

            // wn - Natural angular frequency
            double wn = Math.Sqrt(input.Stiffness / input.Mass);
            double damping = input.DampingRatio * 2 * input.Mass * wn;

            // Velocity of primary object.
            result[0] = y[1];
            // Acceleration of primary object.
            result[1] = (input.Force * Math.Sin(input.AngularFrequency * time) - (damping * y[1]) - (input.Stiffness * y[0])) / input.Mass;

            return Task.FromResult(result);
        }

        /// <summary>
        /// Create a path to the files with the analysis solution.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="requestData"></param>
        /// <param name="analysisType"></param>
        /// <param name="dampingRatio"></param>
        /// <param name="angularFrequency"></param>
        /// <returns></returns>
        public override Task<string> CreateSolutionPath(OneDegreeFreedomResponse response, OneDegreeFreedomRequestData requestData, string analysisType, double dampingRatio, double angularFrequency)
        {
            string path = Directory.GetCurrentDirectory();

            string folderName = Path.GetFileName(path);

            string previousPath = path.Substring(0, path.Length - folderName.Length);

            path = Path.Combine(
                previousPath,
                "Solutions/RigidBody/OneDegreeFreedom",
                $"{analysisType.Trim()}_m={requestData.MechanicalProperties.Mass}_k={requestData.MechanicalProperties.Stiffness}_w={Math.Round(angularFrequency, 2)}_dampingRatio={dampingRatio}.csv");

            if (File.Exists(path))
            {
                response.AddError(ErrorCode.OperationError, $"File already exist in path '{path}'.");
                return null;
            }

            return Task.FromResult(path);
        }
    }
}
