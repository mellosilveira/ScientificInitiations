using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO.Input;
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
            IAuxiliarOperation auxiliarOperation)
            : base(arrayOperation, auxiliarOperation)
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
        public override async Task<double[]> CalculateEquivalentForce(NewmarkMethodInput input, double[] previousDisplacement, double[] previousVelocity, double[] previousAcceleration)
        {
            double[] equivalentVelocity = await CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);
            double[] equivalentAcceleration = await CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions);

            double[,] mass = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];
            double[,] damping = new double[input.NumberOfTrueBoundaryConditions, input.NumberOfTrueBoundaryConditions];

            for (int i = 0; i < input.NumberOfTrueBoundaryConditions; i++)
            {
                for (int j = 0; j < input.NumberOfTrueBoundaryConditions; j++)
                {
                    mass[i, j] = input.Mass[i, j];
                    damping[i, j] = input.Damping[i, j];
                }
            }

            double[] mass_accel = await _arrayOperation.Multiply(mass, equivalentAcceleration, $"{nameof(mass)} and {nameof(equivalentAcceleration)}");
            double[] damping_vel = await _arrayOperation.Multiply(damping, equivalentVelocity, $"{nameof(damping)} and {nameof(equivalentVelocity)}");

            double[] equivalentForce = await _arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}");

            return equivalentForce;
        }
    }
}
