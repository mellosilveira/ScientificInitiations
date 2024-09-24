﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    public class CalculateRectangularBeamWithPiezoelectricVibration : CalculateBeamWithPiezoelectricVibration<RectangularProfile>, ICalculateRectangularBeamWithPiezoelectricVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        /// <param name="mainMatrix"></param>
        public CalculateRectangularBeamWithPiezoelectricVibration(
            IRectangularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            IRectangularProfileValidator profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency,
            IRectangularBeamWithPiezoelectricMainMatrix mainMatrix)
            : base(geometricProperty, mappingResolver, profileValidator, time, naturalFrequency, mainMatrix)
        { }
    }
}
