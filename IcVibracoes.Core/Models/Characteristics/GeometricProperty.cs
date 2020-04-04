namespace IcVibracoes.Core.Models.Characteristics
{
    /// <summary>
    /// It represents the geometric property used for calculations.
    /// </summary>
    public class GeometricProperty
    {
        /// <summary>
        /// Area.
        /// </summary>
        public double[] Area { get; set; }

        /// <summary>
        /// Moment of inertia.
        /// </summary>
        public double[] MomentOfInertia { get; set; }
    }
}
