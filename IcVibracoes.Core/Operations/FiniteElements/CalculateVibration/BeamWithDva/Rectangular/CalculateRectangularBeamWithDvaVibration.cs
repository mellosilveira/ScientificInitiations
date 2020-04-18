using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.Profiles.Rectangular;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark.BeamWithDva;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateRectangularBeamWithDvaVibration : CalculateBeamWithDvaVibration<RectangularProfile>, ICalculateRectangularBeamWithDvaVibration
    {
        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="profileMapper"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="beamMainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CalculateRectangularBeamWithDvaVibration(
            IBeamWithDvaNewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            IRectangularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IRectangularProfileMapper profileMapper,
            IRectangularBeamWithDvaMainMatrix mainMatrix,
            IRectangularBeamMainMatrix beamMainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, mappingResolver, profileValidator, auxiliarOperation, profileMapper, mainMatrix, beamMainMatrix, arrayOperation)
        {
        }
    }
}
