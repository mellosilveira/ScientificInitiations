using IcVibracoes.Common.Classes;
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
        /// It's responsible to build the force vector.
        /// </summary>
        /// <param name="forces"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[]> BuildFrom(List<Force> forces, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to build the electrical charge vector.
        /// </summary>
        /// <param name="electricalCharges"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        Task<double[]> BuildFrom(List<ElectricalCharge> electricalCharges, uint degreesFreedomMaximum);
    }
}