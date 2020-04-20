using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
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

        /// <summary>
        /// Builds the input 'data' of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        Task<DifferentialEquationOfMotionInput> BuildFrom(TwoDegreesFreedomRequestData requestData);

        /// <summary>
        /// Builds the input 'data' of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        Task<DifferentialEquationOfMotionInput> BuildFrom(OneDegreeFreedomRequestData requestData);
    }
}