using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with two degrees freedom case.
    /// </summary>
    public class CalculateVibrationToTwoDegreesFreedom : CalculateVibration_RigidBody<TwoDegreesFreedomRequest, TwoDegreesFreedomRequestData, TwoDegreesFreedomResponse, TwoDegreesFreedomResponseData>, ICalculateVibrationToTwoDegreesFreedom
    {
        public override Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBody_1DF_Variables];

            // wn - Natural angular frequency
            double wn = Math.Sqrt(input.Hardness / input.Mass);
            double secondaryWn = Math.Sqrt(input.SecondaryHardness / input.SecondaryMass);

            double damping = input.DampingRatio * 2 * input.Mass * wn;
            double secondaryDamping = input.DampingRatio * 2 * input.SecondaryMass * secondaryWn;

            // Velocity of primary object.
            result[0] = y[2];
            // Velocity of secondary object.
            result[1] = y[3];
            // Acceleration of primary object.
            result[2] = ((input.Force * Math.Sin(input.AngularFrequency * time)) - ((input.Hardness + input.SecondaryHardness) * y[0] - input.SecondaryHardness * y[1] + (damping + secondaryDamping) * y[2] - secondaryDamping * y[3])) / input.Mass;
            // Acceleration of secondary object.
            result[3] = (input.SecondaryHardness * (y[0] - y[1]) + secondaryDamping * (y[2] - y[3])) / input.SecondaryMass;
            
            return Task.FromResult(result);
        }
    }
}
