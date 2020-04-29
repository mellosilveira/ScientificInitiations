using IcVibracoes.Common.Classes;
using IcVibracoes.Common.ExtensionMethods;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the beam vibration at all contexts.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeam"></typeparam>
    public abstract class CalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam> : OperationBase<TRequest, FiniteElementsResponse, FiniteElementsResponseData>, ICalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam>
        where TRequestData : FiniteElementsRequestData<TProfile>, new()
        where TRequest : FiniteElementsRequest<TProfile, TRequestData>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        private readonly INewmarkBetaMethod _numericalMethod;
        private readonly IProfileValidator<TProfile> _profileValidator;
        private readonly IAuxiliarOperation _auxiliarOperation;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="newmarkBetaMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        public CalculateVibration_FiniteElements(
            INewmarkBetaMethod newmarkBetaMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation)
        {
            this._numericalMethod = newmarkBetaMethod;
            this._profileValidator = profileValidator;
            this._auxiliarOperation = auxiliarOperation;
        }

        /// <summary>
        /// Builds the beam.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TBeam> BuildBeam(TRequest request);

        /// <summary>
        /// Calculates the input to newmark integration method.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        public abstract Task<NewmarkMethodInput> CreateInput(TBeam beam);

        /// <summary>
        /// Createsthe file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<string> CreatePath(TRequest request);

        protected override async Task<FiniteElementsResponse> ProcessOperation(TRequest request)
        {
            FiniteElementsResponse response = new FiniteElementsResponse();

            TBeam beam = await this.BuildBeam(request).ConfigureAwait(false);

            NewmarkMethodInput input = await this.CreateInput(beam).ConfigureAwait(false);

            while(input.AngularFrequency <= request.BeamData.FinalAngularFrequency.Value)
            {
                double time = input.InitialTime;

                string path = await this.CreatePath(request).ConfigureAwait(false);

                AnalysisResult previousResult = new AnalysisResult
                {
                    Displacement = new double[input.DegreesOfFreedom],
                    Velocity = new double[input.DegreesOfFreedom],
                    Acceleration = new double[input.DegreesOfFreedom],
                    Force = input.OriginalForce
                };

                while(time <= input.FinalTime)
                {
                    previousResult.Force = input.OriginalForce.MultiplyEachElement(Math.Cos(input.AngularFrequency * time));

                    AnalysisResult result = await this._numericalMethod.CalculateResult(input, previousResult).ConfigureAwait(false);

                    this._auxiliarOperation.WriteInFile(time, result.Displacement, path);

                    previousResult = result;

                    time += input.TimeStep;
                }

                input.AngularFrequency += request.BeamData.AngularFrequencyStep.Value;
            }

            return response;
        }

        /// <summary>
        /// It's responsible to validade
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async override Task<FiniteElementsResponse> ValidateOperation(TRequest request)
        {
            FiniteElementsResponse response = new FiniteElementsResponse();

            bool isProfileValid = await this._profileValidator.Execute(request.BeamData.Profile, response).ConfigureAwait(false);

            //bool isBeamDataValid;

            if (!isProfileValid)
            {
                return response;
            }

            return response;
        }
    }
}
