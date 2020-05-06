using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.TimeOperation
{
    /// <summary>
    /// It contains operations evolving the time for the analysis.
    /// </summary>
    public interface ITime
    {
        /// <summary>
        /// Calculates the time step for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodDivision"></param>
        /// <returns></returns>
        Task<double> CalculateTimeStep(double angularFrequency, uint periodDivision);

        /// <summary>
        /// Calculates the final time for finite element analysis.
        /// </summary>
        /// <param name="angularFrequency"></param>
        /// <param name="periodCount"></param>
        /// <returns></returns>
        Task<double> CalculateFinalTime(double angularFrequency, uint periodCount);
    }
}
