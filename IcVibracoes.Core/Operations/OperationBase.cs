using IcVibracoes.DataContracts;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations
{
    // TODO: Refatorar 
    public abstract class OperationBase<TRequest, TResponse, TResponseData> : IOperationBase<TRequest, TResponse, TResponseData>
        where TRequest : OperationRequestBase
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
    {
        public async Task<TResponse> Process(TRequest request)
        {
            TResponse response = new TResponse();

            // TODO: Remover o try-catch
            try
            {
                response = await ValidateOperation(request).ConfigureAwait(false);
                if (!response.Success)
                {
                    return response;
                }

                response = await ProcessOperation(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response = new TResponse();
                response.AddError("000", $"{ex.Message}");
            }

            return response;
        }

        protected abstract Task<TResponse> ProcessOperation(TRequest request);

        protected abstract Task<TResponse> ValidateOperation(TRequest request);
    }
}
