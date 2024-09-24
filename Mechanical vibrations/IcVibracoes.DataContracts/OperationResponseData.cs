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
        /// A simple analysis explanation.
        /// </summary>
        public string AnalysisExplanation { get; set; }

        /// <summary>
        /// The file URI with the solution files.
        /// </summary>
        public ICollection<string> FileUris { get; set; }
    }
}
