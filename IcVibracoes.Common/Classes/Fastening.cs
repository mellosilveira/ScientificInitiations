using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It contains the node positions and type of the fastening.
    /// </summary>
    public class Fastening
    {
        /// <summary>
        /// The node position of fastening.
        /// </summary>
        /// <example>0</example>
        [Required]
        public uint NodePosition { get; set; }

        /// <summary>
        /// The type of fastening.
        /// </summary>
        /// <example>Fixed</example>
        [Required]
        public string Type { get; set; }
    }
}
