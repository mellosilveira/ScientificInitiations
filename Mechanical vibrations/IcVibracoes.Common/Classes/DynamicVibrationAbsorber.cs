using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of DynamicVibrationAbsorber.
    /// </summary>
    public class DynamicVibrationAbsorber
    {
        /// <summary>
        /// Mass of DVA.
        /// Unit: kg (kilogram)
        /// </summary>
        /// <example>2</example>
        [Required]
        public double Mass { get; set; }

        /// <summary>
        /// Stiffness of DVA.
        /// Unit: N/m (Newton per meter)
        /// </summary>
        /// <example>500</example>
        [Required]
        public double Stiffness { get; set; }

        /// <summary>
        /// Node position of DVA.
        /// </summary>
        /// <example>1</example>
        [Required]
        public uint NodePosition { get; set; }
    }
}
