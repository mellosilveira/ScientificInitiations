using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.PiezoelectricProfiles.Circular;
using IcVibracoes.Core.Mapper.Profiles.Circular;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles.Circular;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with piezoelectric.
    /// </summary>
    public class CalculateCircularBeamWithPiezoelectricVibration : CalculateBeamWithPiezoelectricVibration<CircularProfile>, ICalculateCircularBeamWithPiezoelectricVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="profileMapper"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CalculateCircularBeamWithPiezoelectricVibration(
            INewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            ICircularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            ICircularProfileMapper profileMapper,
            IPiezoelectricCircularProfileMapper piezoelectricProfileMapper,
            ICircularBeamWithPiezoelectricMainMatrix mainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, mappingResolver, profileValidator, auxiliarOperation, profileMapper, piezoelectricProfileMapper, mainMatrix, arrayOperation)
        {
        }
    }
}
