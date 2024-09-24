﻿using System.Collections.Generic;
using System.Net;

namespace IcVibracoes.DataContracts
{
    public abstract class OperationResponseBase
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        protected OperationResponseBase()
        {
            this.Errors = new List<OperationError>();
        }

        /// <summary>
        /// The success status of operation.
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// The list of errors.
        /// </summary>
        public List<OperationError> Errors { get; protected set; }

        /// <summary>
        /// Set success to true. The HttpStatusCode will be set to 201 (Created).
        /// </summary>
        public void SetSuccessCreated()
        {
            this.HttpStatusCode = HttpStatusCode.Created;
            this.Success = true;
        }

        /// <summary>
        /// Set success to false. The HttpStatusCode will be set to 400 (BadRequest).
        /// </summary>
        public void SetBadRequestError()
        {
            this.HttpStatusCode = HttpStatusCode.BadRequest;
            this.Success = false;
        }

        /// <summary>
        /// Set success to false. The HttpStatusCode will be set to 401 (Unauthorized).
        /// </summary>
        public void SetUnauthorizedError()
        {
            this.HttpStatusCode = HttpStatusCode.Unauthorized;
            this.Success = false;
        }

        /// <summary>
        /// Set Success to false. The HttpStatusCode will be set to 500 (InternalServerError).
        /// </summary>
        public void SetInternalServerError()
        {
            this.HttpStatusCode = HttpStatusCode.InternalServerError;
            this.Success = false;
        }

        /// <summary>
        /// Set Success to false. The HttpStatusCode will be set to 501 (NotImplemented).
        /// </summary>
        public void SetNotImplementedError()
        {
            this.HttpStatusCode = HttpStatusCode.NotImplemented;
            this.Success = false;
        }

        /// <summary>
        /// This method adds error on list of errors.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="httpStatusCode"></param>
        public void AddError(string code, string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            this.Errors.Add(new OperationError(code, message));

            this.HttpStatusCode = httpStatusCode;
            this.Success = false;
        }
    }

    /// <summary>
    /// It contains the content of response for all operations.
    /// </summary>
    /// <typeparam name="TResponseData"></typeparam>
    public class OperationResponseBase<TResponseData> : OperationResponseBase
        where TResponseData : OperationResponseData
    {
        /// <summary>
        /// It represents the 'data' content of all operation response.
        /// </summary>
        public TResponseData Data { get; set; }
    }
}
