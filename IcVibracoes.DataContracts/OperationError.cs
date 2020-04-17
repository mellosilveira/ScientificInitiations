namespace IcVibracoes.DataContracts
{
    public sealed class OperationError
    {
        public OperationError(string code, string message)
        {
            this.Code = code;
            this.Message = message;
        }
        
        public string Message { get; }
        
        public string Code { get; }
    }
}
