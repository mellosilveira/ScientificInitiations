using IcVibracoes.Common.Classes;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts
{
    /// <summary>
    /// It represents the essencial request content to operations.
    /// </summary>
    public class OperationRequestBase
    {
        /// <summary>
        /// The analysis type. 
        /// Example: Circular Beam, Rectangular Beam with DVA, Square Beam With Piezoelectric.
        /// </summary>
        public virtual string AnalysisType { get; }

        /// <summary>
        /// Who is doing the analysis.
        /// </summary>
        /// <example>Bruno Silveira</example>
        [Required]
        public string Author { get; set; }

        /// <summary>
        /// The numerical method that have to be used in the analysis.
        /// </summary>
        /// <example>Newmark</example>
        public NumericalMethod NumericalMethod { get; set; }

        /// <summary>
        /// The force type.
        /// Can be harmonic or impact.
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
        /// Initial angular frequency.
        /// Unit: Hz (Hertz)
        /// </summary>
        /// <example>30</example>
        [Required]
        public double InitialAngularFrequency { get; set; }

        /// <summary>
        /// Delta angular frequency.
        /// Unit: Hz (Hertz)
        /// </summary>
        /// <example>1</example>
        [Required]
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// Final angular frequency.
        /// Unit: Hz (Hertz)
        /// </summary>
        /// <example>50</example>
        [Required]
        public double FinalAngularFrequency { get; set; }
    }
}
