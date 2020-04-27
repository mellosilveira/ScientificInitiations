using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts.FiniteElements;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta
{
    public class NewmarkBetaMethod : INewmarkBetaMethod
    {
        private readonly IArrayOperation _arrayOperation;

        public NewmarkBetaMethod(
            IArrayOperation arrayOperation)
        {
            this._arrayOperation = arrayOperation;
        }

        public async Task<AnalysisResult> ExecuteMethod(NewmarBetaMethodInput input, AnalysisResult previousResult)
        {
            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await this._arrayOperation.InverseMatrix(equivalentStiffness, nameof(equivalentStiffness)).ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousResult).ConfigureAwait(false);

            double[] deltaDisplacement = await this._arrayOperation.Multiply(inversedEquivalentStiffness, equivalentForce).ConfigureAwait(false);

            double[] deltaVelocity = new double[input.DegreesOfFreedom];
            double[] deltaAcceleration = new double[input.DegreesOfFreedom];

            for (int i = 0; i < input.DegreesOfFreedom; i++)
            {
                deltaVelocity[i] = (input.Gama / (input.Beta * input.TimeStep)) * deltaDisplacement[i] - (input.Gama / input.Beta) * previousResult.Velocity[i] + input.TimeStep * (1 - input.Gama / (2 * input.Beta)) * previousResult.Acceleration[i];
                deltaAcceleration[i] = (1 / (input.Beta * Math.Pow(input.TimeStep, 2))) * deltaDisplacement[i] - (1 / (input.Beta * input.TimeStep)) * previousResult.Velocity[i] - (1 / (2 * input.Beta)) * previousResult.Acceleration[i];
            }

            return new AnalysisResult
            {
                Displacement = await this._arrayOperation.Sum(previousResult.Displacement, deltaDisplacement).ConfigureAwait(false),
                Velocity = await this._arrayOperation.Sum(previousResult.Velocity, deltaVelocity).ConfigureAwait(false),
                Acceleration = await this._arrayOperation.Sum(previousResult.Acceleration, deltaAcceleration).ConfigureAwait(false),
                Force = input.Force
            };
        }

        private Task<double[,]> CalculateEquivalentStiffness(NewmarBetaMethodInput input)
        {
            double[,] equivalentStiffness = new double[input.DegreesOfFreedom, input.DegreesOfFreedom];

            double const1 = 1 / (input.Beta * Math.Pow(input.TimeStep, 2));
            double const2 = 1 / (input.Beta * input.TimeStep);

            for (int i = 0; i < input.DegreesOfFreedom; i++)
            {
                for (int j = 0; j < input.DegreesOfFreedom; j++)
                {
                    equivalentStiffness[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j] + input.Stiffness[i, j];
                }
            }

            return Task.FromResult(equivalentStiffness);
        }

        private Task<double[,]> CalculateEquivalentDamping(NewmarBetaMethodInput input)
        {
            double[,] equivalentDamping = new double[input.DegreesOfFreedom, input.DegreesOfFreedom];

            double const1 = 1 / (input.Beta * input.TimeStep);
            double const2 = input.Gama / input.Beta;

            for (int i = 0; i < input.DegreesOfFreedom; i++)
            {
                for (int j = 0; j < input.DegreesOfFreedom; j++)
                {
                    equivalentDamping[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j];
                }
            }

            return Task.FromResult(equivalentDamping);
        }

        private Task<double[,]> CalculateEquivalentMass(NewmarBetaMethodInput input)
        {
            double[,] equivalentMass = new double[input.DegreesOfFreedom, input.DegreesOfFreedom];

            double const1 = 1 / (2 * input.Beta);
            double const2 = input.TimeStep * (1 - (input.Gama / (2 * input.Beta)));

            for (int i = 0; i < input.DegreesOfFreedom; i++)
            {
                for (int j = 0; j < input.DegreesOfFreedom; j++)
                {
                    equivalentMass[i, j] = const1 * input.Mass[i, j] + const2 * input.Damping[i, j];
                }
            }

            return Task.FromResult(equivalentMass);
        }

        private async Task<double[]> CalculateEquivalentForce(NewmarBetaMethodInput input, AnalysisResult previousResult)
        {
            double[,] equivalentDamping = await this.CalculateEquivalentDamping(input).ConfigureAwait(false);
            double[,] equivalentMass = await this.CalculateEquivalentMass(input).ConfigureAwait(false);

            double[] damping_vel = await this._arrayOperation.Multiply(equivalentDamping, previousResult.Velocity).ConfigureAwait(false);
            double[] mass_accel = await this._arrayOperation.Multiply(equivalentMass, previousResult.Acceleration).ConfigureAwait(false);
            double[] deltaForce = await this._arrayOperation.Subtract(input.Force, previousResult.Force).ConfigureAwait(false);
            
            double[] equivalentForce = await this._arrayOperation.Sum(deltaForce, damping_vel, mass_accel, $"{nameof(deltaForce)}, {nameof(damping_vel)} and {nameof(mass_accel)}").ConfigureAwait(false);

            return equivalentForce;
        }
    }
}
