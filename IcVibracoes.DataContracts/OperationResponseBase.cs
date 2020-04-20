using System.Collections.Generic;
using System.Linq;

namespace IcVibracoes.DataContracts
{
    public class OperationResponseBase<TResponseData>
        where TResponseData : OperationResponseData
    {
        public OperationResponseBase()
        {
            Errors = new List<OperationError>();
        }

        public bool Success => !Errors.Any();

        public List<OperationError> Errors { get; }

        public TResponseData Data { get; set; }

        public void AddError(string code, string message)
        {
            Errors.Add(new OperationError(code, message));
        }
    }
}
