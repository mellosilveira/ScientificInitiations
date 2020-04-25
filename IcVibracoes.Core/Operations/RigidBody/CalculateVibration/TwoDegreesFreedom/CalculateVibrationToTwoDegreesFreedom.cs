using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.NaturalFrequency;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.TwoDegreeFreedom;
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
        private readonly INaturalFrequency _naturalFrequency;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="auxiliarOperation"></param>
        /// <param name="rungeKutta"></param>
        public CalculateVibrationToTwoDegreesFreedom(
            IAuxiliarOperation auxiliarOperation,
            IRungeKuttaForthOrderMethod_2DF rungeKutta,
            INaturalFrequency naturalFrequency)
            : base(auxiliarOperation, rungeKutta)
        {
            this._naturalFrequency = naturalFrequency;
        }

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
                AngularFrequency = requestData.AndularFrequencyStep,
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

        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];

            // wn - Natural angular frequency
            double wn = await this._naturalFrequency.Calculate(input.Mass, input.Stiffness).ConfigureAwait(false);
            double secondaryWn = await this._naturalFrequency.Calculate(input.SecondaryMass, input.SecondaryStiffness).ConfigureAwait(false);

            double damping = input.DampingRatio * 2 * input.Mass * wn;
            double secondaryDamping = input.DampingRatio * 2 * input.SecondaryMass * secondaryWn;

            // Velocity of primary object.
            result[0] = y[2];
            // Velocity of secondary object.
            result[1] = y[3];
            // Acceleration of primary object.
            result[2] = ((input.Force * Math.Sin(input.AngularFrequency * time)) - ((input.Stiffness + input.SecondaryStiffness) * y[0] - input.SecondaryStiffness * y[1] + (damping + secondaryDamping) * y[2] - secondaryDamping * y[3])) / input.Mass;
            // Acceleration of secondary object.
            result[3] = (input.SecondaryStiffness * (y[0] - y[1]) + secondaryDamping * (y[2] - y[3])) / input.SecondaryMass;

            return result;
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
