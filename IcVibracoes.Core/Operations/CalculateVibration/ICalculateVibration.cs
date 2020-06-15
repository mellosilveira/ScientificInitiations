using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration to a structure.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public interface ICalculateVibration<TRequest, TResponse, TResponseData, TInput> : IOperationBase<TRequest, TResponse, TResponseData>
        where TRequest : OperationRequestBase
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
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> CreateSolutionPath(TRequest request, TInput input, TResponse response);

        /// <summary>
        /// Creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> CreateMaxValuesPath(TRequest request, TInput input, TResponse response);
    }
}