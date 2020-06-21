using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta
{
    /// <summary>
    /// It's responsible to execute the Newmark-Beta numerical integration method to calculate the vibration.
    /// </summary>
    public interface INewmarkBetaMethod
    {
        /// <summary>
        /// Calculates the result for the initial time.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<FiniteElementResult> CalculateResultForInitialTime(FiniteElementsMethodInput input);

        /// <summary>
        /// Executes the Newmark-Beta numerical integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        Task<FiniteElementResult> CalculateResult(FiniteElementsMethodInput input, FiniteElementResult previousResult);
    }
}