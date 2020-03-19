using IcVibracoes.Common.Classes;
using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper
{
    public interface IMappingResolver
    {
        OperationResponseData BuildFrom(NewmarkMethodResponse output, string author, string analysisExplanation);

        Task<double[]> BuildFrom(List<Force> forces, uint degreesFreedomMaximum);

        Task<double[]> BuildFrom(List<ElectricalCharge> electricalCharges, uint degreesFreedomMaximum);
    }
}