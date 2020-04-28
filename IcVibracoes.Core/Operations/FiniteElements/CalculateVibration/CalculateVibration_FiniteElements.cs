using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
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
        where TProfile : Profile, new()
        where TRequestData : FiniteElementsRequestData<TProfile>, new()
        where TRequest : FiniteElementsRequest<TProfile, TRequestData>
        where TBeam : IBeam<TProfile>, new()
    {
        private readonly INewmarkMethod _newmarkMethod;
        private readonly IProfileValidator<TProfile> _profileValidator;
        private readonly IAuxiliarOperation _auxiliarOperation;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        public CalculateVibration_FiniteElements(
            INewmarkMethod newmarkMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation)
        {
            this._newmarkMethod = newmarkMethod;
            this._profileValidator = profileValidator;
            this._auxiliarOperation = auxiliarOperation;
        }

        /// <summary>
        /// It's responsible to build the beam.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TBeam> BuildBeam(TRequest request, uint degreesFreedomMaximum);

        /// <summary>
        /// It's responsible to calculate the input to newmark integration method.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="newmarkMethodParameter"></param>
        /// <returns></returns>
        public abstract Task<NewmarkMethodInput> CreateInput(TBeam beam, NewmarkMethodParameter newmarkMethodParameter, uint degreesFreedomMaximum);

        protected override async Task<FiniteElementsResponse> ProcessOperation(TRequest request)
        {
            //FiniteElementsResponse response = new FiniteElementsResponse();

            //uint degreesFreedomMaximum = this._auxiliarOperation.CalculateDegreesFreedomMaximum(request.BeamData.NumberOfElements);

            //TBeam beam = await this.BuildBeam(request, degreesFreedomMaximum).ConfigureAwait(false);

            //NewmarkMethodInput input = await this.CreateInput(beam, request.MethodParameterData, degreesFreedomMaximum).ConfigureAwait(false);

            //await this._newmarkMethod.CalculateResponse(input, response, request.AnalysisType, request.BeamData.NumberOfElements).ConfigureAwait(false);

            //return response;

            FiniteElementsResponse response = new FiniteElementsResponse();



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
