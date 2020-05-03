using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
using IcVibracoes.Core.Validators.Profiles.Circular;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateCircularBeamWithDvaVibration : CalculateBeamWithDvaVibration<CircularProfile>, ICalculateCircularBeamWithDvaVibration
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
        public CalculateCircularBeamWithDvaVibration(
            INewmarkMethod newmarkMethod,
            ICircularProfileValidator profileValidator, 
            IAuxiliarOperation auxiliarOperation, 
            IArrayOperation arrayOperation,
            ICircularGeometricProperty geometricProperty, 
            IMappingResolver mappingResolver, 
            ICircularBeamWithDvaMainMatrix mainMatrix) 
            : base(newmarkMethod, profileValidator, auxiliarOperation, arrayOperation, geometricProperty, mappingResolver, mainMatrix)
        {
        }
    }
}
