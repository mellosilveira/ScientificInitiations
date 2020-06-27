using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
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
        protected virtual Task<TResponse> ValidateOperation(TRequest request)
        {
            TResponse response = new TResponse();

            if (request == null)
            {
                response.AddError(OperationErrorCode.RequestValidationError, "Request cannot be null.");

                return Task.FromResult(response);
            }

            if (string.IsNullOrWhiteSpace(request.Author))
            {
                response.AddError(OperationErrorCode.RequestValidationError, "Author cannot be null or white space.");
            }

            if (string.IsNullOrWhiteSpace(request.NumericalMethod))
            {
                response.AddError(OperationErrorCode.RequestValidationError, "Numerical method cannot be null or white space.");
            }

            if (Enum.TryParse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true, out object _) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid numerical method: '{request.NumericalMethod}'.");
            }

            if (string.IsNullOrWhiteSpace(request.ForceType))
            {
                response.AddError(OperationErrorCode.RequestValidationError, "ForceType cannot be null or white space.");
            }

            if (Enum.TryParse(typeof(ForceType), request.ForceType, ignoreCase: true, out object _) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid force type: '{request.ForceType}'.");
            }

            if (request.PeriodCount <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, "PeriodCount must be greather than zero.");
            }

            if (request.PeriodDivision <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, "PeriodDivision must be greather than zero.");
            }
            
            if(request.InitialAngularFrequency > request.FinalAngularFrequency)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Final angular frequency: '{request.InitialAngularFrequency}' must be grether than initial angular frequency: '{request.InitialAngularFrequency}'.");
            }

            if(request.AngularFrequencyStep == 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Angular frequency step: '{request.AngularFrequencyStep}' cannot be zero.");
            }

            return Task.FromResult(response);
        }

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
