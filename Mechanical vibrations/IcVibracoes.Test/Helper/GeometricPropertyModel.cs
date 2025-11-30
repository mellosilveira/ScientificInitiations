using IcVibracoes.Common.Profiles;

namespace IcVibracoes.Test.Helper
{
    /// <summary>
    /// It contains the requests stub to be used on unit tests.
    /// </summary>
    public static class GeometricPropertyModel
    {
        #region Structure Profiles
        public static CircularProfile CircularBeamProfileWithThickness
            => new CircularProfile()
            {
                Diameter = 3e-3,
                Thickness = 1e-3
            };

        public static CircularProfile CircularBeamProfileWithoutThickness
            => new CircularProfile()
            {
                Diameter = 3e-3
            };

        public static RectangularProfile RectangularBeamProfileWithThickness
            => new RectangularProfile()
            {
                Height = 3e-3,
                Width = 25e-3,
                Thickness = 1e-3
            };

        public static RectangularProfile RectangularBeamProfileWithoutThickness
            => new RectangularProfile()
            {
                Height = 3e-3,
                Width = 25e-3
            };

        #endregion

        #region Piezoelectric Profile

        public static CircularProfile CircularPiezoelectricProfile
            => new CircularProfile()
            {
                Diameter = 0.267e-3
            };

        public static RectangularProfile RectangularPiezoelectricProfile
            => new RectangularProfile()
            {
                Height = 0.267e-3,
                Width = 25e-3
            };

        #endregion

        #region Structure Geometric Properties

        public static double[] CircularAreaWithThickness
            => new double[] { 6.2832E-06, 6.2832E-06 };

        public static double[] CircularAreaWithoutThickness
            => new double[] { 7.0686E-06, 7.0686E-06 };

        public static double[] CircularMomentOfInertiaWithThickness
            => new double[] { 3.927E-12, 3.927E-12 };

        public static double[] CircularMomentOfInertiaWithoutThickness
            => new double[] { 3.976E-12, 3.976E-12 };

        public static double[] RectangularAreaWithThickness
            => new double[] { 5.2E-05, 5.2E-05 };

        public static double[] RectangularAreaWithoutThickness
            => new double[] { 7.5E-05, 7.5E-05 };

        public static double[] RectangularMomentOfInertiaWithoutThickness
            => new double[] { 5.625E-11, 5.625E-11 };

        public static double[] RectangularPiezoelectricMomentOfInertia
            => new double[] { 3.570E-11, 3.570E-11 };

        #endregion

        #region Piezoelectric Geometric Properties

        public static double[] CircularPiezoelectricArea
            => null;

        public static double[] CircularPiezoelectricMomentOfInertia
            => null;

        public static double[] RectangularPiezoelectricArea
            => new double[] { 1.335E-05, 1.335E-05 };

        public static double[] RectangularMomentOfInertiaWithThickness
            => new double[] { 5.433E-11, 5.433E-11 };

        #endregion

        #region Precision to Geometric Property Matrix

        public static double CircularAreaPrecision
            => 1e-10;

        public static double CircularMomentOfInertiaPrecision
            => 1e-15;

        public static double RectangularAreaPrecision 
            => 1e-6;
        
        public static double RectangularMomentOfInertiaPrecision 
            => 1e-14;
        #endregion
    }
}
