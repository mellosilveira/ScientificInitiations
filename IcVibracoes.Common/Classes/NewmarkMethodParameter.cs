namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of newmark method parameter.
    /// </summary>
    public class NewmarkMethodParameter
    {
        /// <summary>
        /// Initial time of analysis.
        /// </summary>
        public double InitialTime { get; set; }

        /// <summary>
        /// Divisions on period.
        /// </summary>
        public uint PeriodDivision { get; set; }

        /// <summary>
        /// Number of periods.
        /// </summary>
        public uint NumberOfPeriods { get; set; }

        /// <summary>
        /// Initial angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double InitialAngularFrequency { get; set; }

        /// <summary>
        /// Delta angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double? DeltaAngularFrequency { get; set; }

        /// <summary>
        /// Final angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double? FinalAngularFrequency { get; set; }
    }
}
