﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Circular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular profile beam.
    /// </summary>
    public class CalculateCircularBeamVibration : CalculateBeamVibration<CircularProfile>, ICalculateCircularBeamVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="boundaryCondition"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="profileValidator"></param>
        /// <param name="file"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateCircularBeamVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            ICircularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            ICircularBeamMainMatrix mainMatrix,
            ICircularProfileValidator profileValidator,
            IFile file,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(boundaryCondition, arrayOperation, geometricProperty, mappingResolver, mainMatrix, profileValidator, file, time, naturalFrequency)
        { }
    }
}
