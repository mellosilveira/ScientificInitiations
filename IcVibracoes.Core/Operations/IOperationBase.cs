using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations
{
    /// <summary>
    /// It represents the base for all operations in the application.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public interface IOperationBase<TRequest, TRequestData, TResponse, TResponseData>
        where TRequest : OperationRequestBase<TRequestData>
        where TRequestData : OperationRequestData
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
    {
        /// <summary>
        /// The main method of all operations.
        /// This method orchestrates the operations.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TResponse> Process(TRequest request);
    }
}
