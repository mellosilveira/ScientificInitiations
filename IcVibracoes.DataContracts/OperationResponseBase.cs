using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IcVibracoes.DataContracts
{
    /// <summary>
    /// It contains the content of response for all operations.
    /// </summary>
    /// <typeparam name="TResponseData"></typeparam>
    public class OperationResponseBase<TResponseData>
        where TResponseData : OperationResponseData
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        public OperationResponseBase()
        {
            this.Errors = new List<OperationError>();
        }

        /// <summary>
        /// The success status of operation.
        /// </summary>
        public bool Success => !this.Errors.Any();

        /// <summary>
        /// The list of errors.
        /// </summary>
        public List<OperationError> Errors { get; }

        /// <summary>
        /// It represents the 'data' content of all operation response.
        /// </summary>
        public TResponseData Data { get; set; }

        /// <summary>
        /// This method add error on list of errors.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void AddError(string code, string message)
        {
            this.Errors.Add(new OperationError(code, message));
        }
    }
}
