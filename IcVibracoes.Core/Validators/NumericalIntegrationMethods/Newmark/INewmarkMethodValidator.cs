using IcVibracoes.Core.DTO.Input;
using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to validate the parameters used in NewmarkMethod class.
    /// </summary>
    public interface INewmarkMethodValidator
    {
        /// <summary>
        /// Validate the parameters used in the method CalculateResponse that weren't validated previously.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<bool> ValidateParameters(NewmarkMethodInput input, OperationResponseBase response);
    }
}
