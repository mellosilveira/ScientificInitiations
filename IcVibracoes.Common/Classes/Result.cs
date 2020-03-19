namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the result for each time iteration.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Iteration time.
        /// </summary>
        public double[] Time { get; set; }

        /// <summary>
        /// Iteration displacement (linear and angular) for each node.
        /// </summary>
        public double[,] Displacements { get; set; }

        /// <summary>
        /// Iteration velocity (linear and angular) for each node.
        /// </summary>
        public double[,] Velocities { get; set; }

        /// <summary>
        /// Iteration aceleration (linear and angular) for each node.
        /// </summary>
        public double[,] Accelerations { get; set; }

        /// <summary>
        /// Force iteration for each node.
        /// </summary>
        public double[,] Forces { get; set; }
    }
}