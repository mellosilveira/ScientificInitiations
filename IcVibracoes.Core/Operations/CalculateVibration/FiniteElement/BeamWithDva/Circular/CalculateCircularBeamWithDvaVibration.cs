using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Circular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular profile beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateCircularBeamWithDvaVibration : CalculateBeamWithDvaVibration<CircularProfile>, ICalculateCircularBeamWithDvaVibration
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        /// <param name="mainMatrix"></param>
        public CalculateCircularBeamWithDvaVibration(
            ICircularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            ICircularProfileValidator profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency,
            ICircularBeamWithDvaMainMatrix mainMatrix)
            : base(geometricProperty, mappingResolver, profileValidator, time, naturalFrequency, mainMatrix)
        { }
    }
}
