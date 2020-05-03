using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta
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
        Task<AnalysisResult> CalculateResultForInitialTime(NewmarkMethodInput input);

        /// <summary>
        /// Executes the Newmark-Beta numerical integration method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        Task<AnalysisResult> CalculateResult(NewmarkMethodInput input, AnalysisResult previousResult);
    }
}