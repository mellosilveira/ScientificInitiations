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
        public double Diameter { get; set; }

        /// <summary>
        /// Profile thickness.
        /// </summary>
        public double? Thickness { get; set; }
    }
}
