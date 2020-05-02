using IcVibracoes.Common.Classes;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper
{
    /// <summary>
    /// It's responsible to map the data into a specific class.
    /// </summary>
    public class MappingResolver : IMappingResolver
    {
        /// <summary>
        /// This method builds the force vector.
        /// </summary>
        /// <param name="forces"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<double[]> BuildForceVector(List<Force> forces, uint degreesFreedomMaximum)
        {
            if (forces == null)
            {
                return null;
            }

            double[] force = new double[degreesFreedomMaximum];
            foreach (Force applyedForce in forces)
            {
                force[2 * applyedForce.NodePosition] = applyedForce.Value;
            }

            return Task.FromResult(force);
        }

        /// <summary>
        /// This method builds the electrical charge array.
        /// </summary>
        /// <param name="electricalCharges"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<double[]> BuildElectricalChargeVector(List<ElectricalCharge> electricalCharges, uint degreesFreedomMaximum)
        {
            if (electricalCharges == null)
            {
                return null;
            }

            double[] electricalCharge = new double[degreesFreedomMaximum];
            foreach (ElectricalCharge eC in electricalCharges)
            {
                electricalCharge[2 * eC.NodePosition] = eC.Value;
            }

            return Task.FromResult(electricalCharge);
        }

        /// <summary>
        /// Thid method builds the fastenings of the beam.
        /// </summary>
        /// <param name="fastenings"></param>
        /// <returns></returns>
        public Task<IDictionary<uint, FasteningType>> BuildFastenings(List<Fastening> fastenings)
        {
            IDictionary<uint, FasteningType> beamFastenings = new Dictionary<uint, FasteningType>();

            foreach (var fastening in fastenings)
            {
                beamFastenings.Add(fastening.NodePosition, FasteningFactory.Create(fastening.Type));
            }

            return Task.FromResult(beamFastenings);
        }
    }
}
