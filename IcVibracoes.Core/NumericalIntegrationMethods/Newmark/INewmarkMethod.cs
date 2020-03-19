using IcVibracoes.Core.DTO.Input;
using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to execute the Newmark numerical integration method to calculate the vibration.
    /// </summary>
    public interface INewmarkMethod
    {
        /// <summary>
        /// Calculates and write in a file the response matrixes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task CalculateResponse(NewmarkMethodInput input, OperationResponseBase response);
    }
}
