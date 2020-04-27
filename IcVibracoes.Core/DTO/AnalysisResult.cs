namespace IcVibracoes.Core.DTO
{
    /// <summary>
    /// It contains the analysis results for a specific time.
    /// </summary>
    public class AnalysisResult
    {
        public double[] Displacement { get; set; }

        public double[] Velocity { get; set; }

        public double[] Acceleration { get; set; }

        public double[] Force { get; set; }
    }
}
