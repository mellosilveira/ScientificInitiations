using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts.FiniteElements;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark
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
        private readonly IAuxiliarOperation _auxiliarOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public NewmarkMethod(
            IArrayOperation arrayOperation,
            IAuxiliarOperation auxiliarOperation)
        {
            this._arrayOperation = arrayOperation ?? throw new ArgumentNullException(nameof(IArrayOperation), $"'{nameof(IArrayOperation)}' cannot be null in '{GetType().Name}'.");
            this._auxiliarOperation = auxiliarOperation ?? throw new ArgumentNullException(nameof(IAuxiliarOperation), $"'{nameof(IAuxiliarOperation)}' cannot be null in '{GetType().Name}'.");
        }

        /// <summary>
        /// Calculates the response of Newmark numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <param name="analysisType"></param>
        /// <returns></returns>
        public async Task CalculateResponse(NewmarkMethodInput input, FiniteElementsResponse response, string analysisType, uint numberOfElements)
        {
            int numberOfLoops;
            if (input.Parameter.AngularFrequencyStep != default)
            {
                numberOfLoops = (int)((input.Parameter.FinalAngularFrequency - input.Parameter.InitialAngularFrequency) / input.Parameter.AngularFrequencyStep) + 1;
            }
            else
            {
                numberOfLoops = 1;
            }

            path = this._auxiliarOperation.CreateSolutionPath(analysisType, input.Parameter.InitialAngularFrequency, input.Parameter.FinalAngularFrequency, numberOfElements);

            //Parallel.For
            for (int i = 0; i < numberOfLoops; i++)
            {
                if (input.Parameter.AngularFrequencyStep != null)
                {
                    input.AngularFrequency = input.Parameter.InitialAngularFrequency + i * input.Parameter.AngularFrequencyStep.Value;
                }
                else
                {
                    input.AngularFrequency = input.Parameter.InitialAngularFrequency;
                }

                if (input.AngularFrequency != 0)
                {
                    input.TimeStep = Math.PI * 2 / input.AngularFrequency / input.Parameter.PeriodDivision;
                }
                else
                {
                    input.TimeStep = Math.PI * 2 / input.Parameter.PeriodDivision;
                }

                CalculateIngrationContants(input.TimeStep);

                this._auxiliarOperation.WriteInFile(input.AngularFrequency, path);
                await Solution(input).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Constains the Newmark numerical integration logic to calculate the response matrixes.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task Solution(NewmarkMethodInput input)
        {
            double time = input.Parameter.InitialTime;

            double[] y = new double[input.NumberOfTrueBoundaryConditions];
            double[] yPre = new double[input.NumberOfTrueBoundaryConditions];

            double[] vel = new double[input.NumberOfTrueBoundaryConditions];
            double[] velPre = new double[input.NumberOfTrueBoundaryConditions];

            double[] accel = new double[input.NumberOfTrueBoundaryConditions];
            double[] accelPre = new double[input.NumberOfTrueBoundaryConditions];

            //double[] forcePre = new double[input.NumberOfTrueBoundaryConditions];

            for (int jp = 0; jp < input.Parameter.NumberOfPeriods; jp++)
            {
                for (int jn = 0; jn < input.Parameter.PeriodDivision; jn++)
                {
                    for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                    {
                        // Force can't initiate in 0 (?)
                        input.Force[i] = input.OriginalForce[i] * Math.Cos(input.AngularFrequency * time);
                    }

                    if (time != 0)
                    {
                        y = await this.CalculateDisplacement(input, yPre, velPre, accelPre).ConfigureAwait(false);

                        // Parallel.For
                        for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                        {
                            accel[i] = a0 * (y[i] - yPre[i]) - a2 * velPre[i] - a3 * accelPre[i];
                            vel[i] = velPre[i] + a6 * accelPre[i] + a7 * accel[i];
                        }
                    }

                    this._auxiliarOperation.WriteInFile(time, y, path);

                    time += input.TimeStep;

                    for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                    {
                        yPre[i] = y[i];
                        velPre[i] = vel[i];
                        accelPre[i] = accel[i];
                        //forcePre[i] = input.Force[i];
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the displacement to Newmark method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <returns></returns>
        public async Task<double[]> CalculateDisplacement(NewmarkMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration)
        {
            double[,] equivalentStiffness = await this.CalculateEquivalentStiffness(input.Mass, input.Stiffness, input.Damping, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[,] inversedEquivalentStiffness = await this._arrayOperation.InverseMatrix(equivalentStiffness, nameof(equivalentStiffness)).ConfigureAwait(false);

            double[] equivalentForce = await this.CalculateEquivalentForce(input, previousDisplacement, previousVelocity, previousAcceleration).ConfigureAwait(false);

            return await this._arrayOperation.Multiply(inversedEquivalentStiffness, equivalentForce, $"{nameof(equivalentForce)}, {nameof(inversedEquivalentStiffness)}").ConfigureAwait(false);
        }

        /// <summary>
        /// Calculates the equivalent force to calculate the displacement to Newmark method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <returns></returns>
        public virtual async Task<double[]> CalculateEquivalentForce(NewmarkMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration)
        {
            double[] equivalentVelocity = await this.CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[] equivalentAcceleration = await this.CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

            double[] mass_accel = await this._arrayOperation.Multiply(input.Mass, equivalentAcceleration, $"{nameof(input.Mass)} and {nameof(equivalentAcceleration)}").ConfigureAwait(false);
            double[] damping_vel = await this._arrayOperation.Multiply(input.Damping, equivalentVelocity, $"{nameof(input.Damping)} and {nameof(equivalentVelocity)}").ConfigureAwait(false);

            double[] equivalentForce = await this._arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}").ConfigureAwait(false);

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

        public void CalculateIngrationContants(double stepTime)
        {
            a0 = 1 / (Constant.Beta * Math.Pow(stepTime, 2));
            a1 = Constant.Gama / (Constant.Beta * stepTime);
            a2 = 1 / (Constant.Beta * stepTime);
            a3 = 1 / (2 * Constant.Beta) - 1;
            a4 = Constant.Gama / Constant.Beta - 1;
            a5 = stepTime / 2 * (Constant.Gama / Constant.Beta - 2);
            a6 = stepTime * (1 - Constant.Gama);
            a7 = Constant.Gama * stepTime;
        }

        //public Task<bool> ValidateTimeStep()
        //{

        //}
    }
}
