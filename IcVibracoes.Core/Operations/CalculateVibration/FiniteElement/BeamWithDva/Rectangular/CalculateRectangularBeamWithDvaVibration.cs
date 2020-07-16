﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular profile beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateRectangularBeamWithDvaVibration : CalculateBeamWithDvaVibration<RectangularProfile>, ICalculateRectangularBeamWithDvaVibration
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
        public CalculateRectangularBeamWithDvaVibration(
            IRectangularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            IRectangularBeamWithDvaMainMatrix mainMatrix,
            IRectangularProfileValidator profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(geometricProperty, mappingResolver, mainMatrix, profileValidator, time, naturalFrequency)
        { }
    }
}
