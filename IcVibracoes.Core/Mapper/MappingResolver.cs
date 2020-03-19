using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper
{
    public class MappingResolver : IMappingResolver
    {
        public OperationResponseData BuildFrom(NewmarkMethodResponse output, string author, string analysisExplanation)
        {
            if (output == null || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(analysisExplanation))
            {
                return null;
            }

            return new OperationResponseData()
            {
                Author = author,
                AnalysisExplanation = analysisExplanation,
                AnalysisResults = output.Analyses
            };
        }

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
