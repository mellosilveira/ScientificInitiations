using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration to a structure.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public interface ICalculateVibration<TRequest, TRequestData, TResponse, TResponseData, TInput> : IOperationBase<TRequest, TRequestData, TResponse, TResponseData>
        where TRequest : OperationRequestBase<TRequestData>
        where TRequestData : OperationRequestData
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
        where TInput : NumericalMethodInput, new()
    {
        /// <summary>
        /// Calculates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TInput> CreateInput(TRequest request);

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="analysisType"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> CreateSolutionPath(string analysisType, TInput input, TResponse response);

        /// <summary>
        /// Creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="analysisType"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> CreateMaxValuesPath(string analysisType, TInput input, TResponse response);
    }
}