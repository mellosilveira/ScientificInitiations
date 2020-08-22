using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
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

        /// <summary>
        /// This method builds a <see cref="FiniteElementMethodInput"/> based on <see cref="TwoDegreesOfFreedomInput"/>.
        /// It is used in the matricial form of two degrees of freedom solution.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<FiniteElementMethodInput> BuildFiniteElementMethodInput(TwoDegreesOfFreedomInput input);

        /// <summary>
        /// This method builds the finite element result from a vector with variables: displacement, velocity and acceleration, and the force value.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        Task<FiniteElementResult> BuildFiniteElementResult(double[] y, double force);

        /// <summary>
        /// This method builds the vector with variables: displacement, velocity and acceleration, from a finite element result.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="finiteElementResult"></param>
        /// <returns></returns>
        Task<double[]> BuildVariableVector(FiniteElementResult finiteElementResult);
    }
}