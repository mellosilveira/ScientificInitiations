using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.Profiles.Rectangular;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular beam.
    /// </summary>
    public class CalculateRectangularBeamVibration : CalculateBeamVibration<RectangularProfile>, ICalculateRectangularBeamVibration
    {
        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="profileMapper"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CalculateRectangularBeamVibration(
            INewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            IRectangularProfileValidator profileValidator,
            IRectangularProfileMapper profileMapper,
            IAuxiliarOperation auxiliarOperation,
            IRectangularBeamMainMatrix mainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, mappingResolver, profileValidator, profileMapper, auxiliarOperation, mainMatrix, arrayOperation)
        {
        }
    }
}
