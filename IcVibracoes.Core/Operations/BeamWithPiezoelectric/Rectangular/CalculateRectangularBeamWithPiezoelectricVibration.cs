using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.Profiles.Rectangular;
using IcVibracoes.Core.NewmarkNumericalIntegration;
using IcVibracoes.Core.Validators.Profiles.Rectangular;
using IcVibracoes.Methods.AuxiliarOperations;

namespace IcVibracoes.Core.Operations.BeamWithPiezoelectric.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    public class CalculateRectangularBeamWithPiezoelectricVibration : CalculateBeamWithPiezoelectricVibration<RectangularProfile>, ICalculateRectangularBeamWithPiezoelectricVibration
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
        public CalculateRectangularBeamWithPiezoelectricVibration(
            INewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            IRectangularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IRectangularProfileMapper profileMapper,
            IRectangularBeamWithPiezoelectricMainMatrix mainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, mappingResolver, profileValidator, auxiliarOperation, profileMapper, mainMatrix, arrayOperation)
        {
        }
    }
}
