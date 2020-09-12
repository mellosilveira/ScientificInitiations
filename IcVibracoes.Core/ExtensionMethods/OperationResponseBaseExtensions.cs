using System;
using System.Collections.Generic;
using IcVibracoes.DataContracts;

namespace IcVibracoes.Core.ExtensionMethods
{
    public static class OperationResponseBaseExtensions
        
    {
        /// <summary>
        /// Add error by conditionExpression.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="expression"></param>
        /// <param name="errorMessage"></param>
        /// <param name="operationErrorCode"></param>
        public static TResponse AddErrorIf<TResponse>(
            this TResponse response, 
            Func<bool> expression, 
            string errorMessage,
            string operationErrorCode = OperationErrorCode.RequestValidationError)
            where TResponse: OperationResponseBase
        {
            if (expression())
            {
                response.AddError(operationErrorCode, errorMessage);
            }

            return response;
        }

        public static TResponse AddErrorIf<TResponse, TRequestData>(
            this TResponse response,
            ICollection<TRequestData> collection,
            Func<TRequestData, bool> conditionExpression,
            Func<TRequestData, string> errorMessageExpression,
            string operationErrorCode = OperationErrorCode.RequestValidationError)
            where TResponse : OperationResponseBase
        {
            foreach (var data in collection)
            {
                if (conditionExpression(data))
                {
                    response.AddError(operationErrorCode, errorMessageExpression(data));
                }
            }

            return response;
        }
    }
}
