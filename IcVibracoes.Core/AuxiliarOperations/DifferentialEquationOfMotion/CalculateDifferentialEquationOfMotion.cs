using IcVibracoes.Core.AuxiliarOperations.ForceOperation;
using IcVibracoes.Core.AuxiliarOperations.NaturalFrequency;
using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.DifferentialEquationOfMotion
{
    /// <summary>
    /// It's responsible to calculate the differential equation of motion.
    /// </summary>
    public class CalculateDifferentialEquationOfMotion : ICalculateDifferentialEquationOfMotion
    {
        private readonly INaturalFrequency _naturalFrequency;
        private readonly IForceOperation _forceOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="naturalFrequency"></param>
        public CalculateDifferentialEquationOfMotion(
            INaturalFrequency naturalFrequency,
            IForceOperation forceOperation)
        {
            this._naturalFrequency = naturalFrequency;
            this._forceOperation = forceOperation;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the one degree of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> ExecuteForOneDegreeOfFreedom(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];

            // wn - Natural angular frequency
            double wn = await this._naturalFrequency.Calculate(input.Mass, input.Stiffness).ConfigureAwait(false);
            double damping = input.DampingRatio * 2 * input.Mass * wn;

            double force = await this._forceOperation.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);

            // Velocity of primary object.
            result[0] = y[1];
            // Acceleration of primary object.
            result[1] = (force - (damping * y[1]) - (input.Stiffness * y[0])) / input.Mass;

            return result;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the two degrees of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> ExecuteForTwoDegreedOfFreedom(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_2DF];

            // wn - Natural angular frequency
            double wn = await this._naturalFrequency.Calculate(input.Mass, input.Stiffness).ConfigureAwait(false);
            double secondaryWn = await this._naturalFrequency.Calculate(input.SecondaryMass, input.SecondaryStiffness).ConfigureAwait(false);

            double damping = input.DampingRatio * 2 * input.Mass * wn;
            double secondaryDamping = input.DampingRatio * 2 * input.SecondaryMass * secondaryWn;

            double force = await this._forceOperation.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);

            // Velocity of primary object.
            result[0] = y[2];
            // Velocity of secondary object.
            result[1] = y[3];
            // Acceleration of primary object.
            result[2] = (force - ((input.Stiffness + input.SecondaryStiffness) * y[0] - input.SecondaryStiffness * y[1] + (damping + secondaryDamping) * y[2] - secondaryDamping * y[3])) / input.Mass;
            // Acceleration of secondary object.
            result[3] = (input.SecondaryStiffness * (y[0] - y[1]) + secondaryDamping * (y[2] - y[3])) / input.SecondaryMass;

            return result;
        }
    }
}
