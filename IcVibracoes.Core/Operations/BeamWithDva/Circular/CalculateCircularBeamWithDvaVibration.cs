using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.Profiles.Circular;
using IcVibracoes.Core.NewmarkNumericalIntegration;
using IcVibracoes.Core.Validators.Profiles.Circular;
using IcVibracoes.Methods.AuxiliarOperations;

namespace IcVibracoes.Core.Operations.BeamWithDva.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateCircularBeamWithDvaVibration : CalculateBeamWithDvaVibration<CircularProfile>, ICalculateCircularBeamWithDvaVibration
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
        public CalculateCircularBeamWithDvaVibration(
            INewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            ICircularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            ICircularProfileMapper profileMapper,
            IBeamWithDvaMainMatrix mainMatrix,
            ICircularBeamMainMatrix beamMainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, mappingResolver, profileValidator, auxiliarOperation, profileMapper, mainMatrix, beamMainMatrix, arrayOperation)
        {
        }
    }
}
