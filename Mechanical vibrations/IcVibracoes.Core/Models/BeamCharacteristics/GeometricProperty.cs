namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It represents the geometric property used for calculations.
    /// </summary>
    public class GeometricProperty
    {
        private GeometricProperty(double[] area, double[] momentOfInertia)
        {
            Area = area;
            MomentOfInertia = momentOfInertia;
        }

        /// <summary>
        /// Area.
        /// </summary>
        public double[] Area { get; }

        /// <summary>
        /// Moment of inertia.
        /// </summary>
        public double[] MomentOfInertia { get; }

        /// <summary>
        /// Create a new instance of <see cref="GeometricProperty"/>.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="momentOfInertia"></param>
        /// <returns></returns>
        public static GeometricProperty Create(double[] area, double[] momentOfInertia) => new GeometricProperty(area, momentOfInertia);

        /// <summary>
        /// Create empty <see cref="GeometricProperty"/>.
        /// </summary>
        public static GeometricProperty Empty => Create(new double[] { }, new double[] { });
    }
}
