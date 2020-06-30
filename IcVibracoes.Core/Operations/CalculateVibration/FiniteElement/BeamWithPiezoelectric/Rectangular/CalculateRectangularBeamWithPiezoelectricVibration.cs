using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Validators.Profiles.Rectangular;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    public class CalculateRectangularBeamWithPiezoelectricVibration : CalculateBeamWithPiezoelectricVibration<RectangularProfile>, ICalculateRectangularBeamWithPiezoelectricVibration
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
        public CalculateRectangularBeamWithPiezoelectricVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IRectangularGeometricProperty geometricProperty,
            IMappingResolver mappingResolver,
            IRectangularBeamWithPiezoelectricMainMatrix mainMatrix,
            IRectangularProfileValidator profileValidator,
            IFile file,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(boundaryCondition, arrayOperation, geometricProperty, mappingResolver, mainMatrix, profileValidator, file, time, naturalFrequency)
        { }
    }
}
