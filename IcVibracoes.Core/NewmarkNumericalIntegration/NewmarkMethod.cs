using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Models;
using IcVibracoes.DataContracts;
using IcVibracoes.Methods.AuxiliarOperations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NewmarkNumericalIntegration
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

        private readonly IArrayOperation _arrayOperation;
        private readonly IAuxiliarOperation _auxiliarOperation;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="auxiliarOperation"></param>
        public NewmarkMethod(
            IArrayOperation arrayOperation,
            IAuxiliarOperation auxiliarOperation)
        {
            this._arrayOperation = arrayOperation;
            this._auxiliarOperation = auxiliarOperation;
        }

        /// <summary>
        /// It's responsivel to generate the response content to Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task CalculateResponse(NewmarkMethodInput input, OperationResponseBase response)
        {
            int angularFrequencyLoopCount;
            if (input.Parameter.DeltaAngularFrequency != default)
            {
                angularFrequencyLoopCount = (int)((input.Parameter.FinalAngularFrequency - input.Parameter.InitialAngularFrequency) / input.Parameter.DeltaAngularFrequency) + 1;
            }
            else
            {
                angularFrequencyLoopCount = 1;
            }

            for (int i = 0; i < angularFrequencyLoopCount; i++)
            {
                if (angularFrequencyLoopCount == 1)
                {
                    input.AngularFrequency = input.Parameter.InitialAngularFrequency;
                }
                else
                {
                    input.AngularFrequency = (input.Parameter.InitialAngularFrequency + i * input.Parameter.DeltaAngularFrequency.Value);
                }

                if (input.AngularFrequency != 0)
                {
                    input.DeltaTime = Math.PI * 2 / input.AngularFrequency / input.Parameter.PeriodDivision;
                }
                else
                {
                    input.DeltaTime = Math.PI * 2 / input.Parameter.PeriodDivision;
                }

                a0 = 1 / (Constants.Beta * Math.Pow(input.DeltaTime, 2));
                a1 = Constants.Gama / (Constants.Beta * input.DeltaTime);
                a2 = 1 / (Constants.Beta * input.DeltaTime);
                a3 = 1 / (2 * Constants.Beta) - 1;
                a4 = (Constants.Gama / Constants.Beta) - 1;
                a5 = (input.DeltaTime / 2) * ((Constants.Beta / Constants.Beta) - 2);
                a6 = input.DeltaTime * (1 - Constants.Gama);
                a7 = Constants.Gama * input.DeltaTime;

                try
                {
                    this._auxiliarOperation.WriteInFile(input.AngularFrequency);
                    await Solution(input);
                }
                catch (Exception ex)
                {
                    response.AddError("000", $"Error executing the solution. {ex.Message}");
                    return;
                }
            }
        }

        private async Task Solution(NewmarkMethodInput input)
        {
            int i, jn, jp;
            double time = input.Parameter.InitialTime;

            double[] force = new double[input.NumberOfTrueBoundaryConditions];
            for (i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                force[i] = input.Force[i];
            }

            double[] y = new double[input.NumberOfTrueBoundaryConditions];
            double[] yAnt = new double[input.NumberOfTrueBoundaryConditions];

            double[] vel = new double[input.NumberOfTrueBoundaryConditions];
            double[] velAnt = new double[input.NumberOfTrueBoundaryConditions];

            double[] accel = new double[input.NumberOfTrueBoundaryConditions];
            double[] accelAnt = new double[input.NumberOfTrueBoundaryConditions];

            double[] forceAnt = new double[input.NumberOfTrueBoundaryConditions];

            for (jp = 0; jp < input.Parameter.NumberOfPeriods; jp++)
            {
                for (jn = 0; jn < input.Parameter.PeriodDivision; jn++)
                {
                    for (i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                    {
                        // Force can't initiate in 0 (?)
                        input.Force[i] = force[i] * Math.Cos(input.AngularFrequency * time);
                    }

                    if (time != 0)
                    {
                        double[,] equivalentHardness = await CalculateEquivalentHardness(input.Mass, input.Damping, input.Hardness, input.NumberOfTrueBoundaryConditions);
                        double[,] inversedEquivalentHardness = await this._arrayOperation.InverseMatrix(equivalentHardness, nameof(equivalentHardness)).ConfigureAwait(false);

                        double[] equivalentForce = await this.CalculateEquivalentForce(input, y, vel, accel, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

                        y = await this._arrayOperation.Multiply(equivalentForce, inversedEquivalentHardness, $"{nameof(equivalentForce)}, {nameof(inversedEquivalentHardness)}");

                        for (i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                        {
                            accel[i] = a0 * (y[i] - yAnt[i]) - a2 * velAnt[i] - a3 * accelAnt[i];
                            vel[i] = velAnt[i] + a6 * accelAnt[i] + a7 * accel[i];
                        }
                    }

                    this._auxiliarOperation.WriteInFile(time, y);

                    time += input.DeltaTime;

                    for (i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
                    {
                        yAnt[i] = y[i];
                        velAnt[i] = vel[i];
                        accelAnt[i] = accel[i];
                        forceAnt[i] = input.Force[i];
                    }
                }
            }
        }

        private async Task<double[]> CalculateEquivalentForce(NewmarkMethodInput input, double[] displacement, double[] velocity, double[] acceleration, uint numberOfTrueBoundaryConditions)
        {
            uint trueBC = numberOfTrueBoundaryConditions;

            if (displacement.Length != trueBC)
            {
                throw new Exception($"Lenth of displacement: {displacement.Length} have to be equals to number of true bondary conditions: {trueBC}.");
            }

            if (velocity.Length != trueBC)
            {
                throw new Exception($"Lenth of velocity: {velocity.Length} have to be equals to number of true bondary conditions: {trueBC}.");
            }

            if (acceleration.Length != trueBC)
            {
                throw new Exception($"Lenth of acceleration: {acceleration.Length} have to be equals to number of true bondary conditions: {trueBC}.");
            }

            if (input.Mass.Length != Math.Pow(trueBC, 2))
            {
                throw new Exception($"Lenth of input mass: {input.Mass.Length} have to be equals to number of true bondary conditions squared: {Math.Pow(trueBC, 2)}.");
            }

            if (input.Damping.Length != Math.Pow(trueBC, 2))
            {
                throw new Exception($"Lenth of input damping: {input.Damping.Length} have to be equals to number of true bondary conditions squared: {Math.Pow(trueBC, 2)}.");
            }

            if (input.Force.Length != trueBC)
            {
                throw new Exception($"Lenth of input force: {input.Force.Length} have to be equals to number of true bondary conditions: {trueBC}.");
            }

            double[] equivalentVelocity = await this.CalculateEquivalentVelocity(displacement, velocity, acceleration, trueBC);
            double[] equivalentAcceleration = await this.CalculateEquivalentAcceleration(displacement, velocity, acceleration, trueBC);

            double[] mass_accel = await this._arrayOperation.Multiply(input.Mass, equivalentAcceleration, $"{nameof(input.Mass)} and {nameof(equivalentAcceleration)}");
            double[] damping_vel = await this._arrayOperation.Multiply(input.Damping, equivalentVelocity, $"{nameof(input.Damping)} and {nameof(equivalentVelocity)}");

            double[] equivalentForce = await this._arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}");

            return equivalentForce;
        }

        private Task<double[]> CalculateEquivalentAcceleration(double[] displacement, double[] velocity, double[] acceleration, uint numberOfTrueBondaryConditions)
        {
            double[] equivalentAcceleration = new double[numberOfTrueBondaryConditions];

            for (int i = 0; i < numberOfTrueBondaryConditions; i++)
            {
                equivalentAcceleration[i] = a0 * displacement[i] + a2 * velocity[i] + a3 * acceleration[i];
            }

            return Task.FromResult(equivalentAcceleration);
        }

        private Task<double[]> CalculateEquivalentVelocity(double[] displacement, double[] velocity, double[] acceleration, uint numberOfTrueBondaryConditions)
        {
            double[] equivalentVelocity = new double[numberOfTrueBondaryConditions];

            for (int i = 0; i < numberOfTrueBondaryConditions; i++)
            {
                equivalentVelocity[i] = a1 * displacement[i] + a4 * velocity[i] + a5 * acceleration[i];
            }

            return Task.FromResult(equivalentVelocity);
        }

        private Task<double[,]> CalculateEquivalentHardness(double[,] mass, double[,] damping, double[,] hardness, uint numberOfTrueBoundaryConditions)
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
