using IcVibracoes.Core.Calculator.Force;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;

namespace IcVibracoes.Core.Calculator.DifferentialEquationOfMotion
{
    /// <summary>
    /// It's responsible to calculate the differential equation of motion.
    /// </summary>
    public class DifferentialEquationOfMotion : IDifferentialEquationOfMotion
    {
        private readonly IForce _force;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="naturalFrequency"></param>
        public DifferentialEquationOfMotion(
            IForce force)
        {
            this._force = force;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the one degree of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public double[] CalculateForOneDegreeOfFreedom(OneDegreeOfFreedomInput input, double time, double[] previousResult)
        {
            double[] result = new double[Constants.NumberOfRigidBodyVariables1Df];

            double force = this._force.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType);

            // Velocity of primary object.
            result[0] = previousResult[1];
            // Acceleration of primary object.
            result[1] = (force - input.Damping * previousResult[1] - input.Stiffness * previousResult[0]) / input.Mass;

            return result;
        }

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the two degrees of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public double[] CalculateForTwoDegreedOfFreedom(TwoDegreesOfFreedomInput input, double time, double[] previousResult)
        {
            double[] result = new double[Constants.NumberOfRigidBodyVariables2Df];

            double force = this._force.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType);

            // Velocity of primary object.
            result[0] = previousResult[2];

            // Velocity of secondary object.
            result[1] = previousResult[3];

            // Acceleration of primary object.
            result[2] = (force - ((input.Stiffness + input.SecondaryStiffness) * previousResult[0] - input.SecondaryStiffness * previousResult[1] + (input.Damping + input.SecondaryDamping) * previousResult[2] - input.SecondaryDamping * previousResult[3])) / input.Mass;
           
            // Acceleration of secondary object.
            result[3] = (input.SecondaryStiffness * (previousResult[0] - previousResult[1]) + input.SecondaryDamping * (previousResult[2] - previousResult[3])) / input.SecondaryMass;

            return result;
        }
    }
}
