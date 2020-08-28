using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods
{
    /// <summary>
    /// It is responsible to execute the numerical integration method to generate the analysis results.
    /// </summary>
    public interface INumericalIntegrationMethod
    {
        /// <summary>
        /// Calculates the result for the initial time for a finite element analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<FiniteElementResult> CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input);

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task<FiniteElementResult> CalculateFiniteElementResult(FiniteElementMethodInput input, FiniteElementResult previousResult, double time);

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        Task<double[]> CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult);

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        Task<double[]> CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time, double[] previousResult);
    }
}