using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Methods.AuxiliarOperations;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorbers.
    /// </summary>
    public class BeamWithDvaNewmarkMethod : NewmarkMethod, IBeamWithDvaNewmarkMethod
    {
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="validator"></param>
        public BeamWithDvaNewmarkMethod(
            IArrayOperation arrayOperation,
            IAuxiliarOperation auxiliarOperation,
            INewmarkMethodValidator validator)
            : base(arrayOperation, auxiliarOperation, validator) 
        {
            this._arrayOperation = arrayOperation;
        }


        /// <summary>
        /// It's responsible to calculate the equivalent force to a beam with dynamic vibration absorbers.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousDisplacement"></param>
        /// <param name="previousVelocity"></param>
        /// <param name="previousAcceleration"></param>
        /// <returns></returns>
        protected override async Task<double[]> CalculateEquivalentForce(NewmarkMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration)
        {
            if (previousDisplacement.Length != input.NumberOfTrueBoundaryConditions)
            {
                throw new Exception($"Lenth of displacement: {previousDisplacement.Length} have to be equals to number of true bondary conditions: {input.NumberOfTrueBoundaryConditions}.");
            }

            if (previousVelocity.Length != input.NumberOfTrueBoundaryConditions)
            {
                throw new Exception($"Lenth of velocity: {previousVelocity.Length} have to be equals to number of true bondary conditions: {input.NumberOfTrueBoundaryConditions}.");
            }

            if (previousAcceleration.Length != input.NumberOfTrueBoundaryConditions)
            {
                throw new Exception($"Lenth of acceleration: {previousAcceleration.Length} have to be equals to number of true bondary conditions: {input.NumberOfTrueBoundaryConditions}.");
            }

            double[] equivalentVelocity = await CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);
            double[] equivalentAcceleration = await CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);

            double[,] mass = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];
            double[,] damping = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < input.NumberOfTrueBoundaryConditions; j++)
                {
                    try
                    {
                        mass[i, j] = input.Mass[i, j];
                        damping[i, j] = input.Damping[i, j];
                    }
                    catch
                    {
                        throw new ArgumentOutOfRangeException($"Error creating mass and damping matrixes. Stoped in position: {i}, {j}.");
                    }
                }
            }

            double[] mass_accel = await _arrayOperation.Multiply(mass, equivalentAcceleration, $"{nameof(mass)} and {nameof(equivalentAcceleration)}");
            double[] damping_vel = await _arrayOperation.Multiply(damping, equivalentVelocity, $"{nameof(damping)} and {nameof(equivalentVelocity)}");

            double[] equivalentForce = await _arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}");

            return equivalentForce;
        }
    }
}
