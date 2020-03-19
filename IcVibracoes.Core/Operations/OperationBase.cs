using IcVibracoes.DataContracts;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations
{
    public abstract class OperationBase<TRequest, TResponse> : IOperationBase<TRequest, TResponse>
        where TRequest: OperationRequestBase
        where TResponse: OperationResponseBase, new() 
    {
        public async Task<TResponse> Process(TRequest request)
        {
            TResponse response = new TResponse();

            try
            {
                response = await ValidateOperation(request);
                if(!response.Success)
                {
                    return response;
                }

                response = await ProcessOperation(request);
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
