using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Models.Beam.Characteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper.Profiles.Circular
{
    /// <summary>
    /// It's responsible to build a circular profile.
    /// </summary>
    public class CircularProfileMapper : ProfileMapper<CircularProfile>, ICircularProfileMapper
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly ICalculateGeometricProperty _calculateGeometricProperty;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateGeometricProperty"></param>
        public CircularProfileMapper(
            IArrayOperation arrayOperation,
            ICalculateGeometricProperty calculateGeometricProperty)
        {
            this._arrayOperation = arrayOperation;
            this._calculateGeometricProperty = calculateGeometricProperty;
        }

        public async override Task<GeometricProperty> Execute(CircularProfile profile, uint degreesFreedomMaximum)
        {
            GeometricProperty geometricProperty = new GeometricProperty();

            double area = await this._calculateGeometricProperty.Area(profile.Diameter, profile.Thickness.Value);
            double momentOfInertia = await this._calculateGeometricProperty.MomentOfInertia(profile.Diameter, profile.Thickness.Value);

            geometricProperty.Area = await this._arrayOperation.Create(area, degreesFreedomMaximum);
            geometricProperty.MomentOfInertia = await this._arrayOperation.Create(momentOfInertia, degreesFreedomMaximum);

            return geometricProperty;
        }
    }
}
