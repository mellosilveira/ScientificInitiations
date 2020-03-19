using IcVibracoes.Common.Classes;
using System.Collections.Generic;

namespace IcVibracoes.DataContracts
{
    /// <summary>
    /// It represents the 'data' content of all operation response.
    /// </summary>
    public class OperationResponseData
    {
        /// <summary>
        /// Who made the analysis.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A simple analysis explanation .
        /// </summary>
        public string AnalysisExplanation { get; set; }

        /// <summary>
        /// The analysis results for all angular frequency analyzed.
        /// </summary>
        public List<Analysis> AnalysisResults { get; set; }
    }
}
