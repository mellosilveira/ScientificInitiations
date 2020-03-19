using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NewmarkNumericalIntegration
{
    public interface INewmarkMethod
    {
        Task CalculateResponse(NewmarkMethodInput input, OperationResponseBase response);
    }
}
