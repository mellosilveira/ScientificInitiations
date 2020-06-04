using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with dynamic vibration absorber.
    /// </summary>
    public class CalculateCircularBeamWithDvaVibration : CalculateBeamWithDvaVibration<CircularProfile, NewmarkMethodInput>, ICalculateCircularBeamWithDvaVibration
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="boundaryCondition"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="file"></param>
        /// <param name="time"></param>
        /// <param name="newmarkMethod"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateCircularBeamWithDvaVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            ICircularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            ICircularBeamWithDvaMainMatrix mainMatrix,
            IFile file,
            ITime time,
            INewmarkMethod newmarkMethod,
            INaturalFrequency naturalFrequency)
            : base(boundaryCondition, arrayOperation, geometricProperty, mappingResolver, mainMatrix, file, time, newmarkMethod, naturalFrequency)
        {
        }
    }
}
