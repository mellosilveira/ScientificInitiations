using IcVibracoes.DataContracts;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations
{
    // TODO: Refatorar 
    /// <summary>
    /// It represents the base for all operations in the application.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public abstract class OperationBase<TRequest, TResponse, TResponseData> : IOperationBase<TRequest, TResponse, TResponseData>
        where TRequest : OperationRequestBase
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
    {
        /// <summary>
        /// This method processes the operation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected abstract Task<TResponse> ProcessOperation(TRequest request);

        /// <summary>
        /// This method validates the operation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected abstract Task<TResponse> ValidateOperation(TRequest request);

        /// <summary>
        /// The main method of all operations.
        /// This method orchestrates the operations.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TResponse> Process(TRequest request)
        {
            TResponse response = new TResponse();

            try
            {
                response = await this.ValidateOperation(request).ConfigureAwait(false);
                if (!response.Success)
                {
                    return response;
                }

                response = await this.ProcessOperation(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response = new TResponse();
                response.AddError("000", $"{ex.Message}");
            }

            return response;
        }
    }
}
