using IcVibracoes.Common.Classes;
using System;
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
        /// It's responsible to build the force vector.
        /// </summary>
        /// <param name="forces"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<double[]> BuildFrom(List<Force> forces, uint degreesFreedomMaximum)
        {
            if (forces == null)
            {
                return null;
            }

            double[] force = new double[degreesFreedomMaximum];
            foreach (Force applyedForce in forces)
            {
                try
                {
                    force[2 * (applyedForce.NodePosition)] = applyedForce.Value;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating force matrix. {ex.Message}.");
                }
            }

            return Task.FromResult(force);
        }

        /// <summary>
        /// It's responsible to build the electrical charge array.
        /// </summary>
        /// <param name="electricalCharges"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public Task<double[]> BuildFrom(List<ElectricalCharge> electricalCharges, uint degreesFreedomMaximum)
        {
            if (electricalCharges == null)
            {
                return null;
            }

            double[] electricalCharge = new double[degreesFreedomMaximum];
            foreach (ElectricalCharge eC in electricalCharges)
            {
                try
                {
                    electricalCharge[2 * (eC.NodePosition)] = eC.Value;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating electrical charge matrix. {ex.Message}.");
                }
            }

            return Task.FromResult(electricalCharge);
        }
    }
}
