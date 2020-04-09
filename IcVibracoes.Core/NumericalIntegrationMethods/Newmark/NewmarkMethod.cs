using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark;
using IcVibracoes.DataContracts;
using IcVibracoes.Methods.AuxiliarOperations;
using System;
using System.IO;
using System.Text;
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
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly INewmarkMethodValidator _validator;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        public NewmarkMethod(
            IArrayOperation arrayOperation,
            IAuxiliarOperation auxiliarOperation,
            INewmarkMethodValidator validator)
        {
            this._arrayOperation = arrayOperation ?? throw new ArgumentNullException("", nameof(IArrayOperation));
            this._auxiliarOperation = auxiliarOperation ?? throw new ArgumentNullException("", nameof(IAuxiliarOperation));
            this._validator = validator ?? throw new ArgumentNullException("", nameof(INewmarkMethodValidator));
        }

        /// <summary>
        /// Calculates the response of Newmark numerical integration.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <param name="analysisType"></param>
        /// <returns></returns>
        public async Task CalculateResponse(NewmarkMethodInput input, OperationResponseBase response, string analysisType, uint numberOfElements)
        {
            if (!await this._validator.ValidateParameters(input, response))
            {
                return;
            }

            int numberOfLoops;
            if (input.Parameter.DeltaAngularFrequency != default)
            {
                numberOfLoops = (int)((input.Parameter.FinalAngularFrequency - input.Parameter.InitialAngularFrequency) / input.Parameter.DeltaAngularFrequency) + 1;
            }
            else
            {
                numberOfLoops = 1;
            }

            path = this._auxiliarOperation.CreateSolutionPath(analysisType, input.Parameter.InitialAngularFrequency, input.Parameter.FinalAngularFrequency, numberOfElements);

            //Parallel.For
            for (int i = 0; i < numberOfLoops; i++)
            {
                if (input.Parameter.DeltaAngularFrequency != null)
                {
                    input.AngularFrequency = (input.Parameter.InitialAngularFrequency + i * input.Parameter.DeltaAngularFrequency.Value);
                }
                else
                {
                    input.AngularFrequency = input.Parameter.InitialAngularFrequency;
                }

                if (input.AngularFrequency != 0)
                {
                    input.DeltaTime = Math.PI * 2 / input.AngularFrequency / input.Parameter.PeriodDivision;
                }
                else
                {
                    input.DeltaTime = Math.PI * 2 / input.Parameter.PeriodDivision;
                }

                a0 = 1 / (Constant.Beta * Math.Pow(input.DeltaTime, 2));
                a1 = Constant.Gama / (Constant.Beta * input.DeltaTime);
                a2 = 1 / (Constant.Beta * input.DeltaTime);
                a3 = 1 / (2 * Constant.Beta) - 1;
                a4 = (Constant.Gama / Constant.Beta) - 1;
                a5 = (input.DeltaTime / 2) * ((Constant.Beta / Constant.Beta) - 2);
                a6 = input.DeltaTime * (1 - Constant.Gama);
                a7 = Constant.Gama * input.DeltaTime;

                try
                {
                    this._auxiliarOperation.WriteInFile(input.AngularFrequency, path);
                    await Solution(input);
                }
                catch (Exception ex)
                {
                    response.AddError(ErrorCode.NewmarkMethod, $"Error executing the solution. {ex.Message}");
                    return;
                }
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

            double[] force = new double[input.NumberOfTrueBoundaryConditions];
            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                force[i] = input.Force[i];
            }

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
                        input.Force[i] = force[i] * Math.Cos(input.AngularFrequency * time);
                    }

                    if (time != 0)
                    {
                        y = await CalculateDisplacement(input, yPre, velPre, accelPre);

                        // Parallel.For
                        for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                        {
                            accel[i] = (a0 * (y[i] - yPre[i])) - (a2 * velPre[i]) - (a3 * accelPre[i]);
                            vel[i] = velPre[i] + (a6 * accelPre[i]) + (a7 * accel[i]);
                        }
                    }

                    this._auxiliarOperation.WriteInFile(time, y, path);

                    time += input.DeltaTime;

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
            double[,] equivalentHardness = await CalculateEquivalentHardness(input.Mass, input.Hardness, input.Damping, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[,] inversedEquivalentHardness = await this._arrayOperation.InverseMatrix(equivalentHardness, nameof(equivalentHardness)).ConfigureAwait(false);

            double[] equivalentForce = await CalculateEquivalentForce(input, previousDisplacement, previousVelocity, previousAcceleration).ConfigureAwait(false);

            return await this._arrayOperation.Multiply(inversedEquivalentHardness, equivalentForce, $"{nameof(equivalentForce)}, {nameof(inversedEquivalentHardness)}");
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
            double[] equivalentVelocity = await CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);
            double[] equivalentAcceleration = await CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);

            double[] mass_accel = await this._arrayOperation.Multiply(input.Mass, equivalentAcceleration, $"{nameof(input.Mass)} and {nameof(equivalentAcceleration)}");
            double[] damping_vel = await this._arrayOperation.Multiply(input.Damping, equivalentVelocity, $"{nameof(input.Damping)} and {nameof(equivalentVelocity)}");

            double[] equivalentForce = await this._arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}");

            return equivalentForce;
        }

        /// <summary>
        /// Calculates the equivalent aceleration to calculate the equivalent force.
        /// </summary>
        /// <param name="displacement"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="numberOfTrueBondaryConditions"></param>
        /// <returns></returns>
        public Task<double[]> CalculateEquivalentAcceleration(double[] displacement, double[] velocity, double[] acceleration, uint numberOfTrueBondaryConditions)
        {
            double[] equivalentAcceleration = new double[numberOfTrueBondaryConditions];

            for (int i = 0; i < numberOfTrueBondaryConditions; i++)
            {
                equivalentAcceleration[i] = a0 * displacement[i] + a2 * velocity[i] + a3 * acceleration[i];
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
        /// Calculates the equivalent hardness to calculate the displacement in Newmark method.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="damping"></param>
        /// <param name="hardness"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public Task<double[,]> CalculateEquivalentHardness(double[,] mass, double[,] hardness, double[,] damping, uint numberOfTrueBoundaryConditions)
        {
            double[,] equivalentHardness = new double[numberOfTrueBoundaryConditions, numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < numberOfTrueBoundaryConditions; j++)
                {
                    equivalentHardness[i, j] = a0 * mass[i, j] + a1 * damping[i, j] + hardness[i, j];
                }
            }

            return Task.FromResult(equivalentHardness);
        }
    }
}
