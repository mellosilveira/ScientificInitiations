using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Models;
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
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public Task<double[]> BuildForceVector(List<Force> forces, uint degreesOfFreedom)
        {
            if (forces == null)
            {
                return null;
            }

            double[] force = new double[degreesOfFreedom];
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
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public Task<double[]> BuildElectricalChargeVector(List<ElectricalCharge> electricalCharges, uint degreesOfFreedom)
        {
            if (electricalCharges == null)
            {
                return null;
            }

            double[] electricalCharge = new double[degreesOfFreedom];
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

        /// <summary>
        /// This method builds a <see cref="FiniteElementMethodInput"/> based on <see cref="TwoDegreesOfFreedomInput"/>.
        /// It is used in the matricial form of two degrees of freedom solution.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<FiniteElementMethodInput> BuildFiniteElementMethodInput(TwoDegreesOfFreedomInput input)
        {
            double[,] mass = new double[,] { { input.Mass, 0 }, { 0, input.SecondaryMass } };
            double[,] stiffness = new double[,] { { input.Stiffness + input.SecondaryStiffness, -input.SecondaryStiffness }, { -input.SecondaryStiffness, input.SecondaryStiffness } };
            double[,] damping = new double[,] { { input.Damping + input.SecondaryDamping, -input.SecondaryDamping }, { -input.SecondaryDamping, input.SecondaryDamping } };

            return Task.FromResult(new FiniteElementMethodInput(NumericalMethod.Newmark)
            {
                Mass = mass,
                Damping = damping,
                Stiffness = stiffness,
                OriginalForce = new double[] { input.Force, 0 },
                AngularFrequency = input.AngularFrequency,
                AngularFrequencyStep = input.AngularFrequencyStep,
                FinalAngularFrequency = input.FinalAngularFrequency,
                FinalTime = input.FinalTime,
                NumberOfTrueBoundaryConditions = 2,
                TimeStep = input.TimeStep,
            });
        }

        /// <summary>
        /// This method builds the finite element result from a vector with variables: displacement, velocity and acceleration, and the force value.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public Task<FiniteElementResult> BuildFiniteElementResult(double[] y, double force)
        {
            var result = new FiniteElementResult
            {
                Displacement = new double[] { y[0], y[1] },
                Velocity = new double[] { y[2], y[3] },
                Acceleration = new double[] { y[4], y[5] },
                Force = new double[] { force, 0 }
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// This method builds the vector with variables: displacement, velocity and acceleration, from a finite element result.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="finiteElementResult"></param>
        /// <returns></returns>
        public Task<double[]> BuildVariableVector(FiniteElementResult finiteElementResult)
        {
            double[] y = finiteElementResult.Displacement
                .CombineVectors(finiteElementResult.Velocity)
                .CombineVectors(finiteElementResult.Acceleration);

            return Task.FromResult(y);
        }
    }
}
