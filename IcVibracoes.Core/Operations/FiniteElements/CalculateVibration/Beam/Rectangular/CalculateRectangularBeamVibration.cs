﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular beam.
    /// </summary>
    public class CalculateRectangularBeamVibration : CalculateBeamVibration<RectangularProfile>, ICalculateRectangularBeamVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="newmarkBetaMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        public CalculateRectangularBeamVibration(
            INewmarkBetaMethod newmarkBetaMethod,
            IRectangularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IArrayOperation arrayOperation,
            IRectangularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            IRectangularBeamMainMatrix mainMatrix)
            : base(newmarkBetaMethod, profileValidator, auxiliarOperation, arrayOperation, geometricProperty, mappingResolver, mainMatrix)
        { }
    }
}
