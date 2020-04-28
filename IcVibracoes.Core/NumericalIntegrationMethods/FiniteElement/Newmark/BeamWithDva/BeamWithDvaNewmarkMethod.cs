using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.DTO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark.BeamWithDva
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
            _arrayOperation = arrayOperation;
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
            double[] equivalentVelocity = await this.CalculateEquivalentVelocity(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);
            double[] equivalentAcceleration = await this.CalculateEquivalentAcceleration(previousDisplacement, previousVelocity, previousAcceleration, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

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

            double[] mass_accel = await this._arrayOperation.Multiply(mass, equivalentAcceleration, $"{nameof(mass)} and {nameof(equivalentAcceleration)}").ConfigureAwait(false);
            double[] damping_vel = await this._arrayOperation.Multiply(damping, equivalentVelocity, $"{nameof(damping)} and {nameof(equivalentVelocity)}").ConfigureAwait(false);

            double[] equivalentForce = await this._arrayOperation.Sum(input.Force, mass_accel, damping_vel, $"{nameof(input.Force)}, {nameof(mass_accel)} and {nameof(damping_vel)}").ConfigureAwait(false);

            return equivalentForce;
        }
    }
}
