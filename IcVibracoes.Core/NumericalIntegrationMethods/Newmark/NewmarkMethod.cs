using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using System;
using System.IO;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to execute the Newmark numerical integration method to calculate the vibration.
    /// </summary>
    public class NewmarkMethod : NumericalIntegrationMethod, INewmarkMethod
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mappingResolver"></param>
        public NewmarkMethod(IMappingResolver mappingResolver)
            : base(mappingResolver)
        { }

        /// <summary>
        /// Integration constants.
        /// </summary>
        public double a0, a1, a2, a3, a4, a5, a6, a7;

        /// <summary>
        /// Calculates the result for the initial time for a finite element analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override FiniteElementResult CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input)
        {
            return new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                Force = input.OriginalForce
            };
        }

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override FiniteElementResult CalculateFiniteElementResult(FiniteElementMethodInput input,
            FiniteElementResult previousResult, double time)
        {
            FiniteElementResult result = new FiniteElementResult
            {
                Displacement = new double[input.NumberOfTrueBoundaryConditions],
                Velocity = new double[input.NumberOfTrueBoundaryConditions],
                Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                Force = previousResult.Force
            };

            this.CalculateIngrationContants(input);

            double[,] equivalentStiffness = this.CalculateEquivalentStiffness(input.Mass, input.Stiffness, input.Damping, input.NumberOfTrueBoundaryConditions);
            double[,] inversedEquivalentStiffness = equivalentStiffness.InverseMatrix();

            double[] equivalentForce = this.CalculateEquivalentForce(input, previousResult.Displacement, previousResult.Velocity, previousResult.Acceleration, time);

            result.Displacement = inversedEquivalentStiffness.Multiply(equivalentForce);

            string path = @"C:\Users\bruno\OneDrive\Área de Trabalho\Testes - IC Vibrações\Matrizes resultantes\master.csv";
            using (var streamWriter = new StreamWriter(path))
            {
                WriteMatrix(streamWriter, equivalentStiffness, "Keq");
                WriteMatrix(streamWriter, inversedEquivalentStiffness, "IKeq");

                WriteVector(streamWriter, equivalentForce, "Feq");
                WriteVector(streamWriter, result.Displacement, "Y");
            }

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                result.Acceleration[i] = a0 * (result.Displacement[i] - previousResult.Displacement[i]) - a2 * previousResult.Velocity[i] - a3 * previousResult.Acceleration[i];
                result.Velocity[i] = previousResult.Velocity[i] + a6 * previousResult.Acceleration[i] + a7 * result.Acceleration[i];
            }

            return result;
        }

        private static void WriteMatrix(StreamWriter streamWriter, double[,] matrix, string matrixName)
        {
            streamWriter.Write(string.Format(matrixName));
            streamWriter.Write(streamWriter.NewLine);

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    streamWriter.Write($"{matrix[i, j]}; ");
                }

                streamWriter.Write(streamWriter.NewLine);
            }

            streamWriter.Write(streamWriter.NewLine);
        }

        private static void WriteVector(StreamWriter streamWriter, double[] vector, string matrixName)
        {
            streamWriter.Write(string.Format(matrixName));
            streamWriter.Write(streamWriter.NewLine);

            foreach (var t in vector)
            {
                streamWriter.Write($"{t}; ");
            }

            streamWriter.Write(streamWriter.NewLine);
        }

        /// <summary>
        /// Calculates the equivalent force to calculate the displacement to Newmark method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <returns></returns>
        public double[] CalculateEquivalentForce(FiniteElementMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, double time)
        {
            double[] equivalentVelocity = this.CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);
            double[] equivalentAcceleration = this.CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);

            double[] massAcceleration = input.Mass.Multiply(equivalentAcceleration);
            double[] dampingVel = input.Damping.Multiply(equivalentVelocity);

            double[] equivalentForce = new double[input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                equivalentForce[i] = input.OriginalForce[i] * Math.Sin(input.AngularFrequency * time) + massAcceleration[i] + dampingVel[i];
            }

            return equivalentForce;
        }

        /// <summary>
        /// Calculates the equivalent acceleration to calculate the equivalent force.
        /// </summary>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        private double[] CalculateEquivalentAcceleration(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBoundaryConditions)
        {
            double[] equivalentAcceleration = new double[numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
            {
                equivalentAcceleration[i] = a0 * previousDisplacement[i] + a2 * previousVelocity[i] + a3 * previousAcceleration[i];
            }

            return equivalentAcceleration;
        }

        /// <summary>
        /// Calculates the equivalent velocity to calculate the equivalent force.
        /// </summary>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        private double[] CalculateEquivalentVelocity(double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration, uint numberOfTrueBoundaryConditions)
        {
            double[] equivalentVelocity = new double[numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
            {
                equivalentVelocity[i] = a1 * previousDisplacement[i] + a4 * previousVelocity[i] + a5 * previousAcceleration[i];
            }

            return equivalentVelocity;
        }

        /// <summary>
        /// Calculates the equivalent stiffness to calculate the displacement in Newmark method.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="damping"></param>
        /// <param name="stiffness"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        public double[,] CalculateEquivalentStiffness(double[,] mass, double[,] stiffness, double[,] damping, uint numberOfTrueBoundaryConditions)
        {
            double[,] equivalentStiffness = new double[numberOfTrueBoundaryConditions, numberOfTrueBoundaryConditions];

            for (int i = 0; i < numberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < numberOfTrueBoundaryConditions; j++)
                {
                    equivalentStiffness[i, j] = a0 * mass[i, j] + a1 * damping[i, j] + stiffness[i, j];
                }
            }

            return equivalentStiffness;
        }

        /// <summary>
        /// Calculates the ingration constants to be used in method.
        /// </summary>
        /// <param name="input"></param>
        private void CalculateIngrationContants(NumericalMethodInput input)
        {
            this.a0 = 1 / (input.Beta * Math.Pow(input.TimeStep, 2));
            this.a1 = input.Gama / (input.Beta * input.TimeStep);
            this.a2 = 1 / (input.Beta * input.TimeStep);
            this.a3 = 1 / (2 * input.Beta) - 1;
            this.a4 = input.Gama / input.Beta - 1;
            this.a5 = (input.TimeStep / 2) * (input.Gama / input.Beta - 2);
            this.a6 = input.TimeStep * (1 - input.Gama);
            this.a7 = input.Gama * input.TimeStep;
        }

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis using Newmark integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public override double[] CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult)
        {
            this.CalculateIngrationContants(input);

            double equivalentStiffness = input.Stiffness + this.a0 * input.Mass + this.a1 * input.DampingRatio;
            double equivalentForce =
                input.Force * Math.Sin(input.AngularFrequency * time)
                + input.Mass * (this.a0 * previousResult[0] + this.a2 * previousResult[1] + this.a3 * previousResult[2])
                + input.Damping * (this.a1 * previousResult[0] + this.a4 * previousResult[1] + this.a5 * previousResult[2]);

            var result = new double[Constants.NumberOfRigidBodyVariables1Df];
            
            // Displacement
            result[0] = equivalentForce / equivalentStiffness;
            
            // Acceleration
            result[2] = this.a0 * (result[0] - previousResult[0]) - this.a2 * previousResult[1] - this.a3 * previousResult[2];
            
            // Velocity
            result[1] = previousResult[1] + this.a6 * previousResult[2] + this.a7 * result[2];

            return result;
        }
    }
}
