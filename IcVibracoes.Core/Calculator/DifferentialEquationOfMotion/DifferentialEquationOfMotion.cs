using IcVibracoes.Core.Calculator.Force;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.DifferentialEquationOfMotion
{
    /// <summary>
    /// It's responsible to calculate the differential equation of motion.
    /// </summary>
    public class DifferentialEquationOfMotion : IDifferentialEquationOfMotion
    {
        private readonly INaturalFrequency _naturalFrequency;
        private readonly IForce _force;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="naturalFrequency"></param>
        public DifferentialEquationOfMotion(
            INaturalFrequency naturalFrequency,
            IForce force)
        {
            this._naturalFrequency = naturalFrequency;
            this._force = force;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the one degree of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateForOneDegreeOfFreedom(OneDegreeOfFreedomInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];

            double force = await this._force.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);

            // Velocity of primary object.
            result[0] = y[1];
            // Acceleration of primary object.
            result[1] = (force - input.Damping * y[1] - input.Stiffness * y[0]) / input.Mass;

            return result;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the two degrees of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateForTwoDegreedOfFreedom(TwoDegreesOfFreedomInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_2DF];

            double force = await this._force.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);

            // Velocity of primary object.
            result[0] = y[2];
            // Velocity of secondary object.
            result[1] = y[3];
            // Acceleration of primary object.
            result[2] = (force - ((input.Stiffness + input.SecondaryStiffness) * y[0] - input.SecondaryStiffness * y[1] + (input.Damping + input.SecondaryDamping) * y[2] - input.SecondaryDamping * y[3])) / input.Mass;
            // Acceleration of secondary object.
            result[3] = (input.SecondaryStiffness * (y[0] - y[1]) + input.SecondaryDamping * (y[2] - y[3])) / input.SecondaryMass;

            return result;
        }
    }
}
