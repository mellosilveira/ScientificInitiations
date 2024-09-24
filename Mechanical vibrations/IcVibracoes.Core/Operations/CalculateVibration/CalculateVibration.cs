using IcVibracoes.Core.DTO.NumericalMethodInput;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
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
    /// <typeparam name="TInput"></typeparam>
    public abstract class CalculateVibration<TRequest, TResponse, TResponseData, TInput> : OperationBase<TRequest, TResponse, TResponseData>, ICalculateVibration<TRequest, TResponse, TResponseData, TInput>
        where TRequest : OperationRequestBase
        where TResponse : OperationResponseBase<TResponseData>, new()
        where TResponseData : OperationResponseData
        where TInput : NumericalMethodInput, new()
    {
        /// <summary>
        /// This method creates the input to numerical integration method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="TInput"/>.</returns>
        public virtual Task<TInput> CreateInputAsync(TRequest request)
        {
            return Task.FromResult(new TInput
            {
                ForceType = (ForceType)Enum.Parse(typeof(ForceType), request.ForceType, ignoreCase: true),
                NumericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true),
                AngularFrequency = request.InitialAngularFrequency,
                // The default angular frequency step is 1 rad/s.
                AngularFrequencyStep = request.AngularFrequencyStep == 0 ? 1 : request.AngularFrequencyStep,
                // If the final angular frequency is not informed, only one iteration must be made, so the final angular frequency receives the initial angular frequency.
                FinalAngularFrequency = request.FinalAngularFrequency == 0 ? request.InitialAngularFrequency : request.FinalAngularFrequency

                //AngularFrequency = 2 * Math.PI * request.InitialAngularFrequency,
                //// The default angular frequency step is 2pi rad/s (1 Hz).
                //AngularFrequencyStep = request.AngularFrequencyStep == 0 ? 2 * Math.PI : 2 * Math.PI * request.AngularFrequencyStep,
                //// If the final angular frequency is not informed, only one iteration must be made, so the final angular frequency receives the initial angular frequency.
                //FinalAngularFrequency = request.FinalAngularFrequency == 0 ? 2 * Math.PI * request.InitialAngularFrequency : 2 * Math.PI * request.FinalAngularFrequency
            });
        }

        /// <summary>
        /// The numerical method.
        /// </summary>
        protected INumericalIntegrationMethod NumericalMethod { get; set; }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public abstract string CreateSolutionPath(TRequest request, TInput input);

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public abstract string CreateMaxValuesPath(TRequest request, TInput input);
    }
}
