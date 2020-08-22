using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods;
using IcVibracoes.DataContracts;
using System;
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
        where TInput : NumericalMethodInput, new()
    {
        /// <summary>
        /// The numerical method.
        /// </summary>
        protected INumericalIntegrationMethod _numericalMethod;

        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="TInput"/>.</returns>
        public virtual Task<TInput> CreateInput(TRequest request)
        {
            return Task.FromResult(new TInput
            {
                NumericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true),
                AngularFrequency = request.InitialAngularFrequency,
                // The default angular frequency step is 1.
                AngularFrequencyStep = request.AngularFrequencyStep == 0 ? 1 : request.AngularFrequencyStep,
                // If the final angular frequency is not informed, only one iteration must be made, so the final angular frequency receives the initial angular frequency.
                FinalAngularFrequency = request.FinalAngularFrequency == 0 ? request.InitialAngularFrequency : request.FinalAngularFrequency
            });
        }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public abstract Task<string> CreateSolutionPath(TRequest request, TInput input);

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public abstract Task<string> CreateMaxValuesPath(TRequest request, TInput input);
    }
}
