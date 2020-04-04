using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.CalculateVibration;
using IcVibracoes.Methods.AuxiliarOperations;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations
{
    /// <summary>
    /// It's responsible to calculate the beam vibration at all contexts.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeam"></typeparam>
    public abstract class CalculateVibration<TRequest, TRequestData, TProfile, TBeam> : OperationBase<TRequest, CalculateVibrationResponse>, ICalculateVibration<TRequest, TRequestData, TProfile, TBeam>
        where TProfile : Profile, new()
        where TRequestData : IBeamRequestData<TProfile>, new()
        where TRequest : CalculateVibrationRequest<TProfile, TRequestData>
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
        public CalculateVibration(
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

        protected override async Task<CalculateVibrationResponse> ProcessOperation(TRequest request)
        {
            CalculateVibrationResponse response = new CalculateVibrationResponse();
            
            try
            {
                uint degreesFreedomMaximum = this._auxiliarOperation.CalculateDegreesFreedomMaximum(request.BeamData.NumberOfElements);

                TBeam beam = await this.BuildBeam(request, degreesFreedomMaximum);

                NewmarkMethodInput input = await this.CreateInput(beam, request.MethodParameterData, degreesFreedomMaximum);

                await this._newmarkMethod.CalculateResponse(input, response, request.AnalysisType, request.BeamData.NumberOfElements);

                return response;
            }
            catch(Exception ex)
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
        protected async override Task<CalculateVibrationResponse> ValidateOperation(TRequest request)
        {
            CalculateVibrationResponse response = new CalculateVibrationResponse();

            bool isProfileValid = await this._profileValidator.Execute(request.BeamData.Profile, response);

            //bool isBeamDataValid;

            if (!isProfileValid)
            {
                return response;
            }

            return response;
        }
    }
}
