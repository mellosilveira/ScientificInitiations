using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IcVibracoes.DataContracts
{
    public class OperationResponseBase
    {
        public OperationResponseBase()
        {
            Errors = new List<OperationError>();
        }

        public bool Success => !Errors.Any();

        public List<OperationError> Errors { get; }

        public OperationResponseData Data { get; set; }

        public void AddError(OperationError operationError)
        {
            Errors.Add(operationError);
        }

        public void AddError(string code, string message)
        {
            Errors.Add(new OperationError(code, message));
        }
    }
}
