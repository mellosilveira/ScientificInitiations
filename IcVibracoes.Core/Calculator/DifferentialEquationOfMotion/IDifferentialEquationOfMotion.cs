using IcVibracoes.Core.DTO.InputData;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Calculator.DifferentialEquationOfMotion
{
    /// <summary>
    /// It's responsible to calculate the differential equation of motion.
    /// </summary>
    public interface IDifferentialEquationOfMotion
    {
        /// <summary>
        /// Calculates the value of the differential equation of motion used for the one degree of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Task<double[]> ExecuteForOneDegreeOfFreedom(DifferentialEquationOfMotionInput input, double time, double[] y);

        /// <summary>
        /// Calculates the value of the differential equation of motion used for the two degrees of freedom case for a specific time, based on the force and angular frequency that are passed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Task<double[]> ExecuteForTwoDegreedOfFreedom(DifferentialEquationOfMotionInput input, double time, double[] y);
    }
}