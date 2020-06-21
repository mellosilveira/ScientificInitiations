using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta
{
    /// <summary>
    /// It's responsible to execute the Newmark-Beta numerical integration method to calculate the vibration.
    /// </summary>
    public class NewmarkBetaMethod : NumericalIntegrationMethod, INewmarkBetaMethod
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public NewmarkBetaMethod(
            IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis using Newmark-Beta integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<FiniteElementResult> CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input)
        {
            // [accel] = ([M]^(-1))*([F] - [K]*[displacement] - [C]*[velocity])
            // In initial time, displacement and velocity are zero, so, accel could be calculated by:
            // [accel] = ([M]^(-1))*[F]

            double[,] inversedMass = await this._arrayOperation.InverseMatrix(input.Mass, nameof(input.Mass)).ConfigureAwait(false);

            return new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = await this._arrayOperation.Multiply(inversedMass, input.OriginalForce).ConfigureAwait(false),
                Force = input.OriginalForce
            };
        }

        /// <summary>
        /// Calculates and write in a file the results for a one degree of freedom analysis using Newmark-Beta integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override async Task<FiniteElementResult> CalculateFiniteElementResult(FiniteElementMethodInput input, FiniteElementResult previousResult, double time)
        {
            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await this._arrayOperation.InverseMatrix(equivalentStiffness, nameof(equivalentStiffness)).ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousResult).ConfigureAwait(false);

            double[] deltaDisplacement = await this._arrayOperation.Multiply(inversedEquivalentStiffness, equivalentForce).ConfigureAwait(false);

            double[] deltaVelocity = new double[input.NumberOfTrueBoundaryConditions];
            double[] deltaAcceleration = new double[input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                deltaVelocity[i] = input.Gama / (input.Beta * input.TimeStep) * deltaDisplacement[i] - input.Gama / input.Beta * previousResult.Velocity[i] + input.TimeStep * (1 - input.Gama / (2 * input.Beta)) * previousResult.Acceleration[i];
                deltaAcceleration[i] = 1 / (input.Beta * Math.Pow(input.TimeStep, 2)) * deltaDisplacement[i] - 1 / (input.Beta * input.TimeStep) * previousResult.Velocity[i] - 1 / (2 * input.Beta) * previousResult.Acceleration[i];
            }

            return new FiniteElementResult
            {
                Displacement = await this._arrayOperation.Sum(previousResult.Displacement, deltaDisplacement).ConfigureAwait(false),
                Velocity = await this._arrayOperation.Sum(previousResult.Velocity, deltaVelocity).ConfigureAwait(false),
                Acceleration = await this._arrayOperation.Sum(previousResult.Acceleration, deltaAcceleration).ConfigureAwait(false),
                Force = input.Force
            };
        }

        /// <summary>
        /// Calculates the equivalent stiffness to calculate the displacement in Newmark-Beta method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentStiffness(FiniteElementMethodInput input)
        {
            double[,] equivalentStiffness = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];

            double const1 = 1 / (input.Beta * Math.Pow(input.TimeStep, 2));
            double const2 = 1 / (input.Beta * input.TimeStep);

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < input.NumberOfTrueBoundaryConditions; j++)
                {
                    equivalentStiffness[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j] + input.Stiffness[i, j];
                }
            }

            return Task.FromResult(equivalentStiffness);
        }

        /// <summary>
        /// Calculates the equivalent damping to be used in Newmark-Beta method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentDamping(FiniteElementMethodInput input)
        {
            double[,] equivalentDamping = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];

            double const1 = 1 / (input.Beta * input.TimeStep);
            double const2 = input.Gama / input.Beta;

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < input.NumberOfTrueBoundaryConditions; j++)
                {
                    equivalentDamping[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j];
                }
            }

            return Task.FromResult(equivalentDamping);
        }

        /// <summary>
        /// Calculates the equivalent mass to be used in Newmark-Beta method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentMass(FiniteElementMethodInput input)
        {
            double[,] equivalentMass = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];

            double const1 = 1 / (2 * input.Beta);
            double const2 = input.TimeStep * (1 - input.Gama / (2 * input.Beta));

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < input.NumberOfTrueBoundaryConditions; j++)
                {
                    equivalentMass[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j];
                }
            }

            return Task.FromResult(equivalentMass);
        }

        /// <summary>
        /// Calculates the equivalent force to calculate the displacement in Newmark-Beta method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateEquivalentForce(FiniteElementMethodInput input, FiniteElementResult previousResult)
        {
            double[,] equivalentDamping = await this.CalculateEquivalentDamping(input).ConfigureAwait(false);
            double[,] equivalentMass = await this.CalculateEquivalentMass(input).ConfigureAwait(false);

            double[] damping_vel = await this._arrayOperation.Multiply(equivalentDamping, previousResult.Velocity).ConfigureAwait(false);
            double[] mass_accel = await this._arrayOperation.Multiply(equivalentMass, previousResult.Acceleration).ConfigureAwait(false);
            double[] deltaForce = await this._arrayOperation.Subtract(input.Force, previousResult.Force).ConfigureAwait(false);

            double[] equivalentForce = await this._arrayOperation.Sum(deltaForce, damping_vel, mass_accel, $"{nameof(deltaForce)}, {nameof(damping_vel)} and {nameof(mass_accel)}").ConfigureAwait(false);

            return equivalentForce;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time, double[] y)
        {
            throw new NotImplementedException();
        }
    }
}
