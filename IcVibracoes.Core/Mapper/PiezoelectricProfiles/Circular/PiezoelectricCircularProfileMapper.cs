using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Models.Beam.Characteristics;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper.PiezoelectricProfiles.Circular
{
    /// <summary>
    /// It's responsible to build the piezoelectric circular profile.
    /// </summary>
    public class PiezoelectricCircularProfileMapper : PiezoelectricProfileMapper<CircularProfile>, IPiezoelectricCircularProfileMapper
    {
        private readonly IArrayOperation _arrayOperation;
        private readonly ICalculateGeometricProperty _calculateGeometricProperty;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="arrayOperation"></param>
        /// <param name="calculateGeometricProperty"></param>
        public PiezoelectricCircularProfileMapper(
            IArrayOperation arrayOperation,
            ICalculateGeometricProperty calculateGeometricProperty)
        {
            this._arrayOperation = arrayOperation;
            this._calculateGeometricProperty = calculateGeometricProperty;
        }

        /// <summary>
        /// Method to build the piezoelectric circular profile.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="beamProfile"></param>
        /// <param name="numberOfPiezoelectricsPerElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public override Task<GeometricProperty> Execute(CircularProfile piezoelectricProfile, CircularProfile beamProfile, uint numberOfPiezoelectricsPerElements, uint[] elementsWithPiezoelectric, uint numberOfElements)
        {
            throw new NotImplementedException("Not implemented a geometric property calculation for a circular piezoelectric profile.");
        }
    }
}
