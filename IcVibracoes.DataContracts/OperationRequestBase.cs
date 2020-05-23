using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts
{
    /// <summary>
    /// It represents the essencial request content to operations.
    /// </summary>
    public class OperationRequestBase
    {
        /// <summary>
        /// Who made the analysis.
        /// </summary>
        /// <example>Bruno Silveira</example>
        [Required]
        public string Author { get; set; }

        /// <summary>
        /// The analysis type. 
        /// Example: Circular Beam, Rectangular Beam with DVA, Square Beam With Piezoelectric.
        /// </summary>
        public virtual string AnalysisType { get; }
    }
}
