using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Builds the input 'data' of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public Task<DifferentialEquationOfMotionInput> BuildFrom(TwoDegreesFreedomRequestData requestData)
        {
            if (requestData == null || requestData.MainObjectMechanicalProperties == null || requestData.SecondaryObjectMechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new DifferentialEquationOfMotionInput
            {
                AngularFrequency = requestData.AndularFrequencyStep,
                DampingRatio = requestData.DampingRatioList.FirstOrDefault(),
                Force = requestData.Force,
                Hardness = requestData.MainObjectMechanicalProperties.Hardness,
                Mass = requestData.MainObjectMechanicalProperties.Mass,
                SecondaryHardness = requestData.SecondaryObjectMechanicalProperties.Hardness,
                SecondaryMass = requestData.SecondaryObjectMechanicalProperties.Mass
            });
        }

        /// <summary>
        /// Builds the input 'data' of differential equation of motion.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public Task<DifferentialEquationOfMotionInput> BuildFrom(OneDegreeFreedomRequestData requestData)
        {
            if(requestData == null || requestData.MechanicalProperties == null)
            {
                return null;
            }

            return Task.FromResult(new DifferentialEquationOfMotionInput
            {
                AngularFrequency = requestData.AndularFrequencyStep,
                DampingRatio = requestData.DampingRatioList.FirstOrDefault(),
                Force = requestData.Force,
                Hardness = requestData.MechanicalProperties.Hardness,
                Mass = requestData.MechanicalProperties.Mass
            });
        }
    }
}
