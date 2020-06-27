using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.Core.NumericalIntegrationMethods;
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
    public abstract class CalculateVibration<TRequest, TResponse, TResponseData, TInput> : OperationBase<TRequest, TResponse, TResponseData>, ICalculateVibration<TRequest, TResponse, TResponseData, TInput>
        where TRequest : OperationRequestBase
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
        where TInput : NumericalMethodInput
    {
        /// <summary>
        /// The numerical method.
        /// </summary>
        protected INumericalIntegrationMethod _numericalMethod;

        /// <summary>
        /// Creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TInput> CreateInput(TRequest request);

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public abstract Task<string> CreateSolutionPath(TRequest request, TInput input);

        /// <summary>
        /// Creates the file path to write the maximum values calculated in the analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public abstract Task<string> CreateMaxValuesPath(TRequest request, TInput input);
    }
}
