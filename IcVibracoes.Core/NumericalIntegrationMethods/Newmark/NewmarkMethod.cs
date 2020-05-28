using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to execute the Newmark numerical integration method to calculate the vibration.
    /// </summary>
    public class NewmarkMethod : INewmarkMethod
    {
        /// <summary>
        /// Integration constants.
        /// </summary>
        public double a0, a1, a2, a3, a4, a5, a6, a7;

        /// <summary>
        /// The path of file to write the solutions.
        /// </summary>
        public string path;

        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public NewmarkMethod(
            IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Calculates the result for the initial time.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public Task<FiniteElementResult> CalculateResultForInitialTime(NewmarkMethodInput input, FiniteElementResult previousResult)
        {
            return Task.FromResult(previousResult);
        }

        /// <summary>
        /// Calculates and write in a file the response matrixes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task<FiniteElementResult> CalculateResult(NewmarkMethodInput input, FiniteElementResult previousResult, double time)
        {
            FiniteElementResult result = new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                Force = previousResult.Force
            };

            CalculateIngrationContants(input);

            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input.Mass, input.Stiffness, input.Damping, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await this._arrayOperation.InverseMatrix(equivalentStiffness, nameof(equivalentStiffness)).ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousResult.Displacement, previousResult.Velocity, previousResult.Acceleration, time).ConfigureAwait(false);

            result.Displacement = await this._arrayOperation.Multiply(inversedEquivalentStiffness, equivalentForce, $"{nameof(equivalentForce)}, {nameof(inversedEquivalentStiffness)}").ConfigureAwait(false);

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                result.Acceleration[i] = a0 * (result.Displacement[i] - previousResult.Displacement[i]) - a2 * previousResult.Velocity[i] - a3 * previousResult.Acceleration[i];
                result.Velocity[i] = previousResult.Velocity[i] + a6 * previousResult.Acceleration[i] + a7 * result.Acceleration[i];
            }

            return result;
        }

        /// <summary>
        /// Calculates the equivalent force to calculate the displacement to Newmark method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <returns></returns>
        public virtual async Task<double[]> CalculateEquivalentForce(NewmarkMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, double time)
        {
            double[] equivalentVelocity = await this.CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[] equivalentAcceleration = await this.CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

            double[] mass_accel = await this._arrayOperation.Multiply(input.Mass, equivalentAcceleration, $"{nameof(input.Mass)} and {nameof(equivalentAcceleration)}").ConfigureAwait(false);
            double[] damping_vel = await this._arrayOperation.Multiply(input.Damping, equivalentVelocity, $"{nameof(input.Damping)} and {nameof(equivalentVelocity)}").ConfigureAwait(false);

            double[] equivalentForce = new double[input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                equivalentForce[i] = input.OriginalForce[i] * Math.Sin(input.AngularFrequency * time) + mass_accel[i] + damping_vel[i];
            }

            return equivalentForce;
        }

        /// <summary>
        /// Calculates the equivalent aceleration to calculate the equivalent force.
        /// </summary>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <param name="numberOfTrueBondaryConditions"></param>
        /// <returns></returns>
        public Task<double[]> CalculateEquivalentAcceleration(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBondaryConditions)
        {
            double[] equivalentAcceleration = new double[numberOfTrueBondaryConditions];

            for (int i = 0; i < numberOfTrueBondaryConditions; i++)
            {
                equivalentAcceleration[i] = a0 * previousDisplacement[i] + a2 * previousVelocity[i] + a3 * previousAcceleration[i];
            }

            return Task.FromResult(equivalentAcceleration);
        }

        /// <summary>
        /// Calculates the equivalent velocity to calculate the equivalent force.
        /// </summary>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <param name="numberOfTrueBondaryConditions"></param>
        /// <returns></returns>
        public Task<double[]> CalculateEquivalentVelocity(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBondaryConditions)
        {
            double[] equivalentVelocity = new double[numberOfTrueBondaryConditions];

            for (int i = 0; i < numberOfTrueBondaryConditions; i++)
            {
                equivalentVelocity[i] = a1 * previousDisplacement[i] + a4 * previousVelocity[i] + a5 * previousAcceleration[i];
            }

            return Task.FromResult(equivalentVelocity);
        }

        /// <summary>
        /// Calculates the equivalent stiffness to calculate the displacement in Newmark method.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="damping"></param>
        /// <param name="stiffness"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentStiffness(double[,] mass, double[,] stiffness, double[,] damping, uint numberOfTrueBoundaryConditions)
        {
            double[,] equivalentStiffness = new double[numberOfTrueBoundaryConditions, numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < numberOfTrueBoundaryConditions; j++)
                {
                    equivalentStiffness[i, j] = a0 * mass[i, j] + a1 * damping[i, j] + stiffness[i, j];
                }
            }

            return Task.FromResult(equivalentStiffness);
        }

        public void CalculateIngrationContants(NewmarkMethodInput input)
        {
            a0 = 1 / (input.Beta * Math.Pow(input.TimeStep, 2));
            a1 = input.Gama / (input.Beta * input.TimeStep);
            a2 = 1 / (input.Beta * input.TimeStep);
            a3 = 1 / (2 * input.Beta) - 1;
            a4 = input.Gama / input.Beta - 1;
            a5 = input.TimeStep / 2 * (input.Gama / input.Beta - 2);
            a6 = input.TimeStep * (1 - input.Gama);
            a7 = input.Gama * input.TimeStep;
        }
    }
}
