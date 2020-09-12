using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Collections.Generic;
using System.Linq;

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
        public double[] BuildForceVector(List<Force> forces, uint degreesOfFreedom)
        {
            if (forces == null)
            {
                return null;
            }

            double[] force = new double[degreesOfFreedom];
            foreach (Force appliedForce in forces)
            {
                force[2 * appliedForce.NodePosition] = appliedForce.Value;
            }

            return force;
        }

        /// <summary>
        /// This method builds the electrical charge array.
        /// </summary>
        /// <param name="electricalCharges"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public double[] BuildElectricalChargeVector(List<ElectricalCharge> electricalCharges, uint degreesOfFreedom)
        {
            if (electricalCharges == null)
            {
                return null;
            }

            var electricalCharge = new double[degreesOfFreedom];
            foreach (ElectricalCharge eC in electricalCharges)
            {
                electricalCharge[2 * eC.NodePosition] = eC.Value;
            }

            return electricalCharge;
        }

        /// <summary>
        /// Thid method builds the fastenings of the beam.
        /// </summary>
        /// <param name="fastenings"></param>
        /// <returns></returns>
        public IDictionary<uint, FasteningType> BuildFastenings(List<Fastening> fastenings)
        {
            return fastenings.ToDictionary(fastening => fastening.NodePosition, fastening => FasteningType.Create(fastening.Type));
        }

        /// <summary>
        /// This method builds a <see cref="FiniteElementMethodInput"/> based on <see cref="TwoDegreesOfFreedomInput"/>.
        /// It is used in the matricial form of two degrees of freedom solution.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public FiniteElementMethodInput BuildFiniteElementMethodInput(TwoDegreesOfFreedomInput input)
        {
            var finiteElementMethodInput = new FiniteElementMethodInput
            {
                Mass = new[,] 
                {
                    { input.Mass, 0 }, 
                    { 0, input.SecondaryMass }
                },
                Damping = new[,] 
                {
                    { input.Damping + input.SecondaryDamping, -input.SecondaryDamping}, 
                    { -input.SecondaryDamping, input.SecondaryDamping }
                },
                Stiffness = new[,] 
                {
                    { input.Stiffness + input.SecondaryStiffness, -input.SecondaryStiffness }, 
                    { -input.SecondaryStiffness, input.SecondaryStiffness }
                },
                OriginalForce = new[]
                {
                    input.Force, 0
                },
                AngularFrequency = input.AngularFrequency,
                AngularFrequencyStep = input.AngularFrequencyStep,
                FinalAngularFrequency = input.FinalAngularFrequency,
                FinalTime = input.FinalTime,
                NumberOfTrueBoundaryConditions = 2,
                TimeStep = input.TimeStep,
                NumericalMethod = input.NumericalMethod,
                ForceType = input.ForceType
            };

            return finiteElementMethodInput;
        }

        /// <summary>
        /// This method builds the finite element result from a vector with variables: displacement, velocity and acceleration, and the force value.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public FiniteElementResult BuildFiniteElementResult(double[] result, double force)
        {
            var finiteElementResult = new FiniteElementResult
            {
                Displacement = new[]
                {
                    result[0], result[1]
                },
                Velocity = new[]
                {
                    result[2], result[3]
                },
                Acceleration = new[]
                {
                    result[4], result[5]
                },
                Force = new[]
                {
                    force, 0
                }
            };

            return finiteElementResult;
        }

        /// <summary>
        /// This method builds the vector with variables: displacement, velocity and acceleration, from a finite element result.
        /// This method is used in two degrees os freedom matricial analysis.
        /// </summary>
        /// <param name="finiteElementResult"></param>
        /// <returns></returns>
        public double[] BuildVariableVector(FiniteElementResult finiteElementResult)
        {
            double[] previousResult = finiteElementResult.Displacement
                .CombineVectors(finiteElementResult.Velocity)
                .CombineVectors(finiteElementResult.Acceleration);

            return previousResult;
        }
    }
}
