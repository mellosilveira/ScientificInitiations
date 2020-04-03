using System.Threading.Tasks;

namespace IcVibracoes.Calculator.GeometricProperties
{
    /// <summary>
    /// It's responsible to calculate any geometric property.
    /// </summary>
    public interface ICalculateGeometricProperty
    {
        /// <summary>
        /// Method to calculate the area to circular profile.
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        Task<double> CalculateArea(double diameter, double? thickness);

        /// <summary>
        /// Method to calculate the area to rectangular or square profile.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        Task<double> CalculateArea(double height, double width, double? thickness);

        /// <summary>
        /// Method to calculate the moment of inertia to circular profile.
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        Task<double> CalculateMomentOfInertia(double diameter, double? thickness);

        /// <summary>
        /// Method to calculate the moment of inertia to rectangular or square profile.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        Task<double> CalculateMomentOfInertia(double height, double width, double? thickness);

        /// <summary>
        /// Method to calculate the moment of inertia to rectangular or square piezoelectric profile.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="beamHeight"></param>
        /// <param name="numberOfPiezoelectricsPerElement"></param>
        /// <returns></returns>
        Task<double> CalculatePiezoelectricMomentOfInertia(double height, double width, double beamHeight, uint numberOfPiezoelectricsPerElement);
    }
}
