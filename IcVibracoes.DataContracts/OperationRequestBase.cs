using System;
using System.Collections.Generic;
using System.Text;

namespace IcVibracoes.DataContracts
{
    public class OperationRequestBase
    {
        /// <summary>
        /// Who made the analysis.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A simple analysis explanation .
        /// </summary>
        public string AnalysisExplanation { get; set; }
    }
}
