using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.Methods.AuxiliarOperations;
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
    public abstract class CalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam> : OperationBase<TRequest, FiniteElementsResponse>, ICalculateVibration_FiniteElements<TRequest, TRequestData, TProfile, TBeam>
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
            _newmarkMethod = newmarkMethod;
            _profileValidator = profileValidator;
            _auxiliarOperation = auxiliarOperation;
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
            FiniteElementsResponse response = new FiniteElementsResponse();

            try
            {
                uint degreesFreedomMaximum = _auxiliarOperation.CalculateDegreesFreedomMaximum(request.BeamData.NumberOfElements);

                TBeam beam = await BuildBeam(request, degreesFreedomMaximum);

                NewmarkMethodInput input = await CreateInput(beam, request.MethodParameterData, degreesFreedomMaximum);

                await _newmarkMethod.CalculateResponse(input, response, request.AnalysisType, request.BeamData.NumberOfElements);

                return response;
            }
            catch (Exception ex)
            {
                response.AddError("000", ex.Message);

                return response;
            }
        }

        /// <summary>
        /// It's responsible to validade
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async override Task<FiniteElementsResponse> ValidateOperation(TRequest request)
        {
            FiniteElementsResponse response = new FiniteElementsResponse();

            bool isProfileValid = await _profileValidator.Execute(request.BeamData.Profile, response);

            //bool isBeamDataValid;

            if (!isProfileValid)
            {
                return response;
            }

            return response;
        }
    }
}
