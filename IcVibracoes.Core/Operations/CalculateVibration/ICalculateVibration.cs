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
        where TInput : NumericalMethodInput
    {
        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="TInput"/>.</returns>
        Task<TInput> CreateInput(TRequest request, TResponse response);

        /// <summary>
        /// This method creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The solution path.</returns>
        Task<string> CreateSolutionPath(TRequest request, TInput input);

        /// <summary>
        /// This method creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The maximum values solution path.</returns>
        Task<string> CreateMaxValuesPath(TRequest request, TInput input);
    }
}