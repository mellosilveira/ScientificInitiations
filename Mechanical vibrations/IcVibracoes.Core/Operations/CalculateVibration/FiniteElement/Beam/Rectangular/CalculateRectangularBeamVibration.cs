using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular profile beam.
    /// </summary>
    public class CalculateRectangularBeamVibration : CalculateBeamVibration<RectangularProfile>, ICalculateRectangularBeamVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateRectangularBeamVibration(
            IRectangularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            IRectangularBeamMainMatrix mainMatrix,
            IRectangularProfileValidator profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(geometricProperty, mappingResolver, mainMatrix, profileValidator, time, naturalFrequency)
        { }
    }
}
