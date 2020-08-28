using IcVibracoes.Core.Calculator.Force;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta
{
    /// <summary>
    /// It's responsible to execute the Newmark-Beta numerical integration method to calculate the vibration.
    /// </summary>
    public class NewmarkBetaMethod : NumericalIntegrationMethod, INewmarkBetaMethod
    {
        private readonly IForce _force;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="force"></param>
        public NewmarkBetaMethod(IForce force, IMappingResolver mappingResolver)
            : base(mappingResolver)
        {
            this._force = force;
        }

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis using Newmark-Beta integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Task<FiniteElementResult> CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input)
        {
            return Task.FromResult(new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                Force = input.OriginalForce
            });
        }

        /// <summary>
        /// Calculates and write in a file the results for a one degree of freedom analysis using Newmark-Beta integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override async Task<FiniteElementResult> CalculateFiniteElementResult(FiniteElementMethodInput input, FiniteElementResult previousResult, double time)
        {
            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await equivalentStiffness.InverseMatrixAsync().ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousResult, time).ConfigureAwait(false);

            double[] deltaDisplacement = await inversedEquivalentStiffness.MultiplyAsync(equivalentForce).ConfigureAwait(false);

            double[] deltaVelocity = new double[input.NumberOfTrueBoundaryConditions];
            double[] deltaAcceleration = new double[input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                deltaVelocity[i] = input.Gama / (input.Beta * input.TimeStep) * deltaDisplacement[i] - input.Gama / input.Beta * previousResult.Velocity[i] + input.TimeStep * (1 - input.Gama / (2 * input.Beta)) * previousResult.Acceleration[i];
                deltaAcceleration[i] = (1 / (input.Beta * Math.Pow(input.TimeStep, 2))) * deltaDisplacement[i] - (1 / (input.Beta * input.TimeStep)) * previousResult.Velocity[i] - (1 / (2 * input.Beta)) * previousResult.Acceleration[i];
            }

            return new FiniteElementResult
            {
                Displacement = await previousResult.Displacement.SumAsync(deltaDisplacement).ConfigureAwait(false),
                Velocity = await previousResult.Velocity.SumAsync(deltaVelocity).ConfigureAwait(false),
                Acceleration = await previousResult.Acceleration.SumAsync(deltaAcceleration).ConfigureAwait(false),
                Force = await this._force.CalculateForceByType(input.OriginalForce, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false)
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
            double const2 = input.Gama / (input.Beta * input.TimeStep);

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
            double const2 = -input.TimeStep * (1 - input.Gama / (2 * input.Beta));

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
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateEquivalentForce(FiniteElementMethodInput input, FiniteElementResult previousResult, double time)
        {
            double[,] equivalentDamping = await this.CalculateEquivalentDamping(input).ConfigureAwait(false);
            double[,] equivalentMass = await this.CalculateEquivalentMass(input).ConfigureAwait(false);

            double[] damping_vel = await equivalentDamping.MultiplyAsync(previousResult.Velocity).ConfigureAwait(false);
            double[] mass_accel = await equivalentMass.MultiplyAsync(previousResult.Acceleration).ConfigureAwait(false);

            double[] previousForce = await this._force.CalculateForceByType(input.OriginalForce, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);
            double[] force = await this._force.CalculateForceByType(input.OriginalForce, input.AngularFrequency, time + input.TimeStep, input.ForceType).ConfigureAwait(false);
            double[] deltaForce = await force.SubtractAsync(previousForce).ConfigureAwait(false);

            double[] equivalentForce = await deltaForce.SumAsync(damping_vel, mass_accel).ConfigureAwait(false);

            return equivalentForce;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult)
        {
            double equivalentStiffness = (input.Mass / (input.Beta * Math.Pow(input.TimeStep, 2))) + (input.Gama * input.Damping) / (input.Beta * input.TimeStep) + input.Stiffness;
            double equivalentDamping = (input.Mass / (input.Beta * input.TimeStep)) + (input.Gama * input.Damping / input.Beta);
            double equivalentMass = (1 / (2 * input.Beta)) * input.Mass - input.TimeStep * (1 - input.Gama / (2 * input.Beta)) * input.Damping;
            double deltaForce =
                await this._force.CalculateForceByType(input.Force, input.AngularFrequency, time + input.TimeStep, input.ForceType).ConfigureAwait(false)
                - await this._force.CalculateForceByType(input.Force, input.AngularFrequency, time, input.ForceType).ConfigureAwait(false);

            double deltaDisplacement = (deltaForce + equivalentDamping * previousResult[1] + equivalentMass * previousResult[2]) / equivalentStiffness;

            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];
            // Displacement
            result[0] = previousResult[0] + deltaDisplacement;
            // Velocity
            result[1] = (1 - input.Gama / input.Beta) * previousResult[1] + input.TimeStep * (1 - input.Gama / (2 * input.Beta)) * previousResult[2] + (input.Gama / (input.Beta * input.TimeStep)) * deltaDisplacement;
            // Acceleration
            result[2] = (1 / (input.Beta * Math.Pow(input.TimeStep, 2))) * deltaDisplacement - (1 / (input.Beta * input.TimeStep)) * previousResult[1] + (1 - (1 / (2 * input.Beta))) * previousResult[2];
            // The accelerations satisfy the equations of motion and can be calculated by another form:
            // Acceleration = (Force * Sin(AngularFrequency * (time + TimeStep)) - Damping * Velocity - Stiffness * Displacement) / Mass

            return result;
        }
    }
}
