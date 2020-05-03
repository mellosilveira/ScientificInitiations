using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
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
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        public CalculateCircularBeamWithPiezoelectricVibration(
            INewmarkMethod newmarkMethod,
            ICircularProfileValidator profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IArrayOperation arrayOperation,
            ICircularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            ICircularBeamWithPiezoelectricMainMatrix mainMatrix)
            : base(newmarkMethod, profileValidator, auxiliarOperation, arrayOperation, geometricProperty, mappingResolver, mainMatrix)
        { }
    }
}
