using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Profiles
{
    /// <summary>
    /// It represents the circular profile content of all operations with circular beam.
    /// </summary>
    public class CircularProfile : Profile
    {
        /// <summary>
        /// Profile diameter.
        /// </summary>
        /// <example>10e-3</example>
        [Required]
        public double Diameter { get; set; }

        /// <summary>
        /// Profile thickness.
        /// </summary>
        /// <example>2e-3</example>
        [Required]
        public double? Thickness { get; set; }
    }
}
