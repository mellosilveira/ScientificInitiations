namespace IcVibracoes.DataContracts
{
    public class OperationRequestData
    {
        /// <summary>
        /// Initial time of analysis.
        /// </summary>
        public double InitialTime { get; set; }

        /// <summary>
        /// Divisions on period.
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Number of periods.
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// Initial angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double InitialAngularFrequency { get; set; }

        /// <summary>
        /// Delta angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// Final angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// </summary>
        public double FinalAngularFrequency { get; set; }
    }
}
