using IcVibracoes.Core.Models.BeamCharacteristics;

namespace IcVibracoes.Core.Calculator.Force
{
    /// <summary>
    /// It contains additional operations evolving force.
    /// </summary>
    public interface IForce
    {
        /// <summary>
        /// Calculates the force for a specific time based on its type.
        /// </summary>
        /// <param name="originalForce"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="time"></param>
        /// <param name="forceType"></param>
        /// <returns></returns>
        double CalculateForceByType(double originalForce, double angularFrequency, double time, ForceType forceType);

        /// <summary>
        /// Calculates the force for a specific time based on its type.
        /// </summary>
        /// <param name="originalForce"></param>
        /// <param name="angularFrequency"></param>
        /// <param name="time"></param>
        /// <param name="forceType"></param>
        /// <returns></returns>
        double[] CalculateForceByType(double[] originalForce, double angularFrequency, double time, ForceType forceType);
    }
}
