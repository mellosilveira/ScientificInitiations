using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper
{
    /// <summary>
    /// It's responsible to map the data into a specific class.
    /// </summary>
    public interface IMappingResolver
    {
        /// <summary>
        /// Thid method builds the force vector.
        /// </summary>
        /// <param name="forces"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        Task<double[]> BuildForceVector(List<Force> forces, uint degreesOfFreedom);

        /// <summary>
        /// Thid method builds the electrical charge vector.
        /// </summary>
        /// <param name="electricalCharges"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        Task<double[]> BuildElectricalChargeVector(List<ElectricalCharge> electricalCharges, uint degreesOfFreedom);

        /// <summary>
        /// Thid method builds the fastenings of the beam.
        /// </summary>
        /// <param name="fastenings"></param>
        /// <returns></returns>
        Task<IDictionary<uint, FasteningType>> BuildFastenings(List<Fastening> fastenings);
    }
}