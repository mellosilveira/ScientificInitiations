using IcVibracoes.DataContracts;
using System.Threading.Tasks;


namespace IcVibracoes.Core.Operations
{
    public interface IOperationBase<TRequest, TResponse>
        where TRequest : OperationRequestBase
        where TResponse : OperationResponseBase, new()
    {
        Task<TResponse> Process(TRequest request);
    }
}
