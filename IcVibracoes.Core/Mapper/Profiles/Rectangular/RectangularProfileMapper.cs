using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Models.Beam.Characteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper.Profiles.Rectangular
{
    /// <summary>
    /// It's responsible to build a rectangular profile.
    /// </summary>
    public class RectangularProfileMapper : ProfileMapper<RectangularProfile>, IRectangularProfileMapper
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly ICalculateGeometricProperty _calculateGeometricProperty;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateGeometricProperty"></param>
        public RectangularProfileMapper(
            IArrayOperation arrayOperation,
            ICalculateGeometricProperty calculateGeometricProperty)
        {
            this._calculateGeometricProperty = calculateGeometricProperty;
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Method to build the rectangular profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<GeometricProperty> Execute(RectangularProfile profile, uint degreesFreedomMaximum)
        {
            GeometricProperty geometricProperty = new GeometricProperty();

            double area = await this._calculateGeometricProperty.Area(profile.Height, profile.Width, profile.Thickness);
            double momentOfInertia = await this._calculateGeometricProperty.MomentOfInertia(profile.Height, profile.Width, profile.Thickness);

            geometricProperty.Area = await this._arrayOperation.Create(area, degreesFreedomMaximum, nameof(area));
            geometricProperty.MomentOfInertia = await this._arrayOperation.Create(momentOfInertia, degreesFreedomMaximum, nameof(momentOfInertia));

            return geometricProperty;
        }
    }
}
