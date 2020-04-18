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
            int hardnessLength = input.Hardness.Length;
            int dampingLength = input.Damping.Length;
            int forceLength = input.Force.Length;

            bool isMassValid = await this.ValidateMass(input.Mass, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isHardnessValid = await this.ValidadeHardness(input.Hardness, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isDampingValid = await this.ValidadeDamping(input.Damping, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);
            bool isForceValid = await this.ValidateForce(input.Force, input.NumberOfTrueBoundaryConditions, response).ConfigureAwait(false);

            if (isMassValid != true || isHardnessValid != true || isDampingValid != true || isForceValid != true)
            {
                return false;
            }

            if (massLength != hardnessLength || massLength != dampingLength || hardnessLength != dampingLength)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Length of mass: {massLength}, hardness: {hardnessLength} and damping: {dampingLength} must be equal.");
            }

            if (input.Mass.GetLength(0) < forceLength)
            {
                response.AddError(ErrorCode.NewmarkMethod, $"Length of force vector cannot be greather than number of lines in Mass matrix.");
            }

            if (input.Hardness.GetLength(0) < forceLength)
            {
                response.AddError(ErrorCode.NewmarkMethod, $"Length of force vector cannot be greather than number of lines in Hardness matrix.");
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
        /// Validate the hardness matrix.
        /// </summary>
        /// <param name="hardness"></param>
        /// <param name="numberOfTrueBoundaryConditions"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected Task<bool> ValidadeHardness(double[,] hardness, uint numberOfTrueBoundaryConditions, FiniteElementsResponse response)
        {
            if (hardness == null)
            {
                response.AddError(ErrorCode.NullArgument, "Hardness can't be null.");

                return Task.FromResult(false);
            }

            int hardnessRow = hardness.GetLength(0);
            int hardnessColumn = hardness.GetLength(1);

            if (hardnessRow != hardnessColumn)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Hardness matrix must be a square matrix. Sizes: {hardnessRow}x{hardnessColumn}.");

                return Task.FromResult(false);
            }

            if (hardnessRow < numberOfTrueBoundaryConditions || hardnessColumn < numberOfTrueBoundaryConditions)
            {
                response.AddError(ErrorCode.NewmarkMethodInput, $"Sizes of hardness matrix must be at least equals to {numberOfTrueBoundaryConditions}. Hardness sizes: {hardnessRow}x{hardnessColumn}.");

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
