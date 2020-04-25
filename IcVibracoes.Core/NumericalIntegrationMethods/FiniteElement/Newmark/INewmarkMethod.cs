using IcVibracoes.Core.DTO.Input;
using IcVibracoes.DataContracts.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark
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
        /// <param name="analysisType"></param>
        /// <returns></returns>
        Task CalculateResponse(NewmarkMethodInput input, FiniteElementsResponse response, string analysisType, uint numberOfElements);
    }
}
