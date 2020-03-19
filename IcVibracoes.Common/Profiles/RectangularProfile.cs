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
        public double Height { get; set; }

        /// <summary>
        /// Profile width.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Profile thickness.
        /// </summary>
        public double? Thickness { get; set; }
    }
}
