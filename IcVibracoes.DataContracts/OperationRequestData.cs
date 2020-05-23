using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts
{
    /// <summary>
    /// It represents the essencial request 'data' for operations.
    /// </summary>
    public class OperationRequestData
    {
        /// <summary>
        /// The force type.
        /// </summary>
        /// <example>Harmonic</example>
        [Required]
        public string ForceType { get; set; }

        /// <summary>
        /// Initial time of analysis.
        /// Unit: s (second)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double InitialTime { get; set; }

        /// <summary>
        /// Number of periods.
        /// </summary>
        /// <example>10</example>
        [Required]
        public uint PeriodCount { get; set; }

        /// <summary>
        /// Divisions on period.
        /// </summary>
        /// <example>20</example>
        [Required]
        public uint PeriodDivision { get; set; }

        /// <summary>
        /// Initial angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// Unit: Hz (Hertz) or rad/s (radian per second)
        /// </summary>
        /// <example>30</example>
        [Required]
        public double InitialAngularFrequency { get; set; }

        /// <summary>
        /// Delta angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// Unit: Hz (Hertz) or rad/s (radian per second)
        /// </summary>
        /// <example>1</example>
        [Required]
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// Final angular frequency. Receive in Hz and convert to rad/s to the calculus.
        /// Unit: Hz (Hertz) or rad/s (radian per second)
        /// </summary>
        /// <example>50</example>
        [Required]
        public double FinalAngularFrequency { get; set; }
    }
}
