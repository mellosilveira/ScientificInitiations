using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Profiles
{
    /// <summary>
    /// It represents the rectangular profile content of all operations with circular beam.
    /// </summary>
    public class RectangularProfile : Profile
    {
        /// <summary>
        /// Profile height.
        /// </summary>
        /// <example>10e-3</example>
        [Required]
        public double Height { get; set; }

        /// <summary>
        /// Profile width.
        /// </summary>
        /// <example>10e-3</example>
        [Required]
        public double Width { get; set; }

        /// <summary>
        /// Profile thickness.
        /// </summary>
        /// <example>2e-3</example>
        [Required]
        public double? Thickness { get; set; }
    }
}
