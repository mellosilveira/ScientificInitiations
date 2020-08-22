using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to execute the Newmark numerical integration method to calculate the vibration.
    /// </summary>
    public class NewmarkMethod : NumericalIntegrationMethod, INewmarkMethod
    {
        private readonly IMappingResolver _mappingResolver;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mappingResolver"></param>
        public NewmarkMethod(IMappingResolver mappingResolver)
        {
            this._mappingResolver = mappingResolver;
        }

        /// <summary>
        /// Integration constants.
        /// </summary>
        public double a0, a1, a2, a3, a4, a5, a6, a7;

        /// <summary>
        /// Constants used to calculate the integration constants.
        /// </summary>
        public const double Beta = 0.25;
        public const double Gama = 0.5;

        /// <summary>
        /// Calculates the result for the initial time for a finite element analysis using Newmark integration method.
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
        /// Calculates and write in a file the results for a finite element analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override async Task<FiniteElementResult> CalculateFiniteElementResult(FiniteElementMethodInput input, FiniteElementResult previousResult, double time)
        {
            FiniteElementResult result = new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                Force = previousResult.Force
            };

            this.CalculateIngrationContants(input.TimeStep);

            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input.Mass, input.Stiffness, input.Damping, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await equivalentStiffness.InverseMatrixAsync().ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousResult.Displacement, previousResult.Velocity, previousResult.Acceleration, time).ConfigureAwait(false);

            result.Displacement = await inversedEquivalentStiffness.MultiplyAsync(equivalentForce).ConfigureAwait(false);

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
        public async Task<double[]> CalculateEquivalentForce(FiniteElementMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, double time)
        {
            double[] equivalentVelocity = await this.CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[] equivalentAcceleration = await this.CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

            double[] mass_accel = await input.Mass.MultiplyAsync(equivalentAcceleration).ConfigureAwait(false);
            double[] damping_vel = await input.Damping.MultiplyAsync(equivalentVelocity).ConfigureAwait(false);

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
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public Task<double[]> CalculateEquivalentAcceleration(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBoundaryConditions)
        {
            double[] equivalentAcceleration = new double[numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
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
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public Task<double[]> CalculateEquivalentVelocity(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBoundaryConditions)
        {
            double[] equivalentVelocity = new double[numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
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

        /// <summary>
        /// Calculates the ingration constants to be used in method.
        /// </summary>
        /// <param name="timeStep"></param>
        public void CalculateIngrationContants(double timeStep)
        {
            this.a0 = 1 / (Beta * Math.Pow(timeStep, 2));
            this.a1 = Gama / (Beta * timeStep);
            this.a2 = 1 / (Beta * timeStep);
            this.a3 = 1 / (2 * Beta) - 1;
            this.a4 = Gama / Beta - 1;
            this.a5 = (timeStep / 2) * (Gama / Beta - 2);
            this.a6 = timeStep * (1 - Gama);
            this.a7 = Gama * timeStep;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override Task<double[]> CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult)
        {
            this.CalculateIngrationContants(input.TimeStep);

            double equivalentStiffness = input.Stiffness + this.a0 * input.Mass + this.a1 * input.DampingRatio;
            double equivalentForce =
                input.Force * Math.Sin(input.AngularFrequency * time)
                + input.Mass * (this.a0 * previousResult[0] + this.a2 * previousResult[1] + this.a3 * previousResult[2])
                + input.Damping * (this.a1 * previousResult[0] + this.a4 * previousResult[1] + this.a5 * previousResult[2]);

            var result = new double[Constant.NumberOfRigidBodyVariables_1DF];
            // Displacement
            result[0] = equivalentForce / equivalentStiffness;
            // Acceleration
            result[2] = this.a0 * (result[0] - previousResult[0]) - this.a2 * previousResult[1] - this.a3 * previousResult[2];
            // Velocity
            result[1] = previousResult[1] + this.a6 * previousResult[2] + this.a7 * result[2];

            return Task.FromResult(result);
        }

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override async Task<double[]> CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time, double[] y)
        {
            FiniteElementMethodInput finiteElementMethodInput = await this._mappingResolver.BuildFiniteElementMethodInput(input).ConfigureAwait(false);
            FiniteElementResult previousResult = await this._mappingResolver.BuildFiniteElementResult(y, input.Force).ConfigureAwait(false);

            FiniteElementResult finiteElementResult = await this.CalculateFiniteElementResult(finiteElementMethodInput, previousResult, time).ConfigureAwait(false);

            double[] result = await this._mappingResolver.BuildVariableVector(finiteElementResult).ConfigureAwait(false);

            return result;
        }
    }
}
