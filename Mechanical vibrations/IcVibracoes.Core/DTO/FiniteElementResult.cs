namespace IcVibracoes.Core.DTO
{
    /// <summary>
    /// It contains the finite element analysis results to a specific time.
    /// </summary>
    public class FiniteElementResult
    {
        /// <summary>
        /// The displacement vector.
        /// </summary>
        public double[] Displacement { get; set; }

        /// <summary>
        /// The velocity vector.
        /// </summary>
        public double[] Velocity { get; set; }

        /// <summary>
        /// The acceleration vector.
        /// </summary>
        public double[] Acceleration { get; set; }

        /// <summary>
        /// The force vector.
        /// </summary>
        public double[] Force { get; set; }
    }
}
