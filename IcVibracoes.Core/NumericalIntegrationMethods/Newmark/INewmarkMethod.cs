using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.Newmark
{
    /// <summary>
    /// It's responsible to execute the Newmark numerical integration method to calculate the vibration.
    /// </summary>
    public interface INewmarkMethod
    {
        /// <summary>
        /// Calculates the result for the initial time.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        Task<AnalysisResult> CalculateResultForInitialTime(NewmarkMethodInput input, AnalysisResult previousResult);

        /// <summary>
        /// Calculates and write in a file the response matrixes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task<AnalysisResult> CalculateResult(NewmarkMethodInput input, AnalysisResult previousResult, double time);
    }
}
