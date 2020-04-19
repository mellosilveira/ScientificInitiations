using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibration_RigidBody<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, ICalculateVibrationToOneDegreeFreedom
    {
        public override Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBody_1DF_Variables];

            // wn - Natural angular frequency
            double wn = Math.Sqrt(input.Hardness / input.Mass);
            double damping = input.DampingRatio * 2 * input.Mass * wn;

            // Velocity of primary object.
            result[0] = y[1];
            // Acceleration of primary object.
            result[1] = (input.Force * Math.Sin(input.AngularFrequency * time) - (damping * y[1]) - (input.Hardness * y[0])) / input.Mass;

            return Task.FromResult(result);
        }
    }
}
