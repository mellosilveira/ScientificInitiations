namespace IcVibracoes.DataContracts
{
    public class OperationRequestBase
    {
        /// <summary>
        /// Who made the analysis.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The type of analysis. 
        /// Example: Circular Beam, Rectangular Beam with DVA, Square Beam With Piezoelectric.
        /// </summary>
        public string AnalysisType { get; set; }
    }
}
