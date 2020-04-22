using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to validate the parameters used in NewmarkMethod class.
    /// </summary>
    public class NewmarkMethodValidator : INewmarkMethodValidator
    {
        /// <summary>
        /// Validate the parameters used in the method CalculateResponse that weren't validated previously.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<bool> ValidateParameters(NewmarkMethodInput input, FiniteElementsResponse response)
        {
            int massLength = input.Mass.Length;
            int stiffnessLength = input.Stiffness.Length;
            int dampingLength = input.Damping.Length;
            int forceLength = input.Force.Length;

            bool isMassValid = await this.ValidateMass(input.Mass, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isStiffnessValid = await this.ValidadeStiffness(input.Stiffness, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isDampingValid = await this.ValidadeDamping(input.Damping, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isForceValid = await this.ValidateForce(input.Force, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);

            if (isMassValid != true || isStiffnessValid != true || isDampingValid != true || isForceValid != true)
            {
                return false;
            }

            if (massLength != stiffnessLength || massLength != dampingLength || stiffnessLength != dampingLength)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Length of mass: {massLength}, stiffness: {stiffnessLength} and damping: {dampingLength} must be equal.");
            }

            if (input.Mass.GetLength(0) < forceLength)
            {
                response.AddError(ErrorCode.NewmarkMethod, $"Length of force vector cannot be greather than number of lines in Mass matrix.");
            }

            if (input.Stiffness.GetLength(0) < forceLength)
            {
                response.AddError(ErrorCode.NewmarkMethod, $"Length of force vector cannot be greather than number of lines in Stiffness matrix.");
            }

            if (input.Damping.GetLength(0) < forceLength)
            {
                response.AddError(ErrorCode.NewmarkMethod, $"Length of force vector cannot be greather than number of lines in Damping matrix.");
            }

            if (!response.Success)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate the mass matrix.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <returns></returns>
        protected Task<bool> ValidateMass(double[,] mass, uint numberOfTrueBoundaryConditions, FiniteElementsResponse response)
        {
            if (mass == null)
            {
                response.AddError(ErrorCode.NullArgument, "Mass can't be null.");

                return Task.FromResult(false);
            }

            int massRow = mass.GetLength(0);
            int massColumn = mass.GetLength(1);

            if (massRow != massColumn)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Mass matrix must be a square matrix. Sizes: {massRow}x{massColumn}.");

                return Task.FromResult(false);
            }

            if (massRow < numberOfTrueBoundaryConditions || massColumn < numberOfTrueBoundaryConditions)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Sizes of mass matrix must be at least equals to {numberOfTrueBoundaryConditions}. Mass sizes: {massRow}x{massColumn}.");

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate the stiffness matrix.
        /// </summary>
        /// <param name="stiffness"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected Task<bool> ValidadeStiffness(double[,] stiffness, uint numberOfTrueBoundaryConditions, FiniteElementsResponse response)
        {
            if (stiffness == null)
            {
                response.AddError(ErrorCode.NullArgument, "Stiffness can't be null.");

                return Task.FromResult(false);
            }

            int stiffnessRow = stiffness.GetLength(0);
            int stiffnessColumn = stiffness.GetLength(1);

            if (stiffnessRow != stiffnessColumn)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Stiffness matrix must be a square matrix. Sizes: {stiffnessRow}x{stiffnessColumn}.");

                return Task.FromResult(false);
            }

            if (stiffnessRow < numberOfTrueBoundaryConditions || stiffnessColumn < numberOfTrueBoundaryConditions)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Sizes of stiffness matrix must be at least equals to {numberOfTrueBoundaryConditions}. Stiffness sizes: {stiffnessRow}x{stiffnessColumn}.");

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate the damping matrix.
        /// </summary>
        /// <param name="damping"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected Task<bool> ValidadeDamping(double[,] damping, uint numberOfTrueBoundaryConditions, FiniteElementsResponse response)
        {
            if (damping == null)
            {
                response.AddError(ErrorCode.NullArgument, "Damping can't be null.");

                return Task.FromResult(false);
            }

            int dampingRow = damping.GetLength(0);
            int dampingColumn = damping.GetLength(1);

            if (dampingRow != dampingColumn)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Damping matrix must be a square matrix. Sizes: {dampingRow}x{dampingColumn}.");

                return Task.FromResult(false);
            }

            if (dampingRow < numberOfTrueBoundaryConditions || dampingColumn < numberOfTrueBoundaryConditions)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Sizes of damping matrix must be at least equals to {numberOfTrueBoundaryConditions}. Damping sizes: {dampingRow}x{dampingColumn}.");

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate the force vector.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected Task<bool> ValidateForce(double[] force, uint numberOfTrueBoundaryConditions, FiniteElementsResponse response)
        {
            if (force == null)
            {
                response.AddError(ErrorCode.NullArgument, "Force can't be null.");

                return Task.FromResult(false);
            }

            int forceLength = force.Length;

            if (forceLength < numberOfTrueBoundaryConditions)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Length of force vector: {forceLength} must be at least equals to {numberOfTrueBoundaryConditions}.");

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
