using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Models;
using System.Collections.Generic;

namespace IcVibracoes.Core.DTO
{
    /// <summary>
    /// It represents the 
    /// </summary>
    public class NewmarkMethodResponse
    {
        /// <summary>
        /// Time, displacement, velocity, aceleration and force for each node in the analyzed beam.
        /// </summary>
        public List<Analysis> Analyses { get; set; }
    }
}
