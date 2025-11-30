using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of the applied force .
    /// </summary>
    public class Force
    {
        /// <summary>
        /// Force node position.
        /// </summary>
        /// <example>2</example>
        [Required]
        public uint NodePosition { get; set; }

        /// <summary>
        /// Force value.
        /// Unit: N (Newton)
        /// </summary>
        /// <example>10</example>
        [Required]
        public double Value { get; set; }
    }
}
