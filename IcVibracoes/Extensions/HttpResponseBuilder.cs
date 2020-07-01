using IcVibracoes.DataContracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IcVibracoes.Extensions
{
    /// <summary>
    /// It is responsible to build the HTTP response.
    /// </summary>
    public static class HttpResponseBuilder
    {
        /// <summary>
        /// This method builds the HTTP response .
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static JsonResult BuildHttpResponse<TResponseData>(this OperationResponseBase<TResponseData> response)
            where TResponseData : OperationResponseData
        {
            // BadRequest Status Code.
            if (response.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                response.SetBadRequestError();

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            // Unauthorized Status Code.
            else if (response.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                response.SetUnauthorizedError();

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            // InternalServerError Status Code.
            else if (response.HttpStatusCode == HttpStatusCode.InternalServerError)
            {
                response.SetInternalServerError();

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            // NotImplemented Status Code.
            else if (response.HttpStatusCode == HttpStatusCode.NotImplemented)
            {
                response.SetNotImplementedError();

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.NotImplemented
                };
            }

            response.SetSuccessCreated();
            return new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
