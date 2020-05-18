using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.TimeOperation;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
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
        private readonly INewmarkMethod _numericalMethod;
        private readonly IProfileValidator<TProfile> _profileValidator;
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly ITime _time;

        /// <summary>
        /// Class construtor.
        /// </summary>
        /// <param name="newmarkBetaMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        public CalculateVibration_FiniteElements(
            INewmarkMethod newmarkMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            ITime time)
        {
            this._numericalMethod = newmarkMethod;
            this._profileValidator = profileValidator;
            this._auxiliarOperation = auxiliarOperation;
            this._time = time;
        }

        /// <summary>
        /// Builds the beam.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TBeam> BuildBeam(TRequest request, uint degreesOfFreedom);

        /// <summary>
        /// Calculates the input to newmark integration method.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        public abstract Task<NewmarkMethodInput> CreateInput(TBeam beam, TRequest request, uint degreesOfFreedom);

        /// <summary>
        /// Creates the file path to write the results.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public abstract Task<string> CreatePath(TRequest request, NewmarkMethodInput input, FiniteElementsResponse response);

        protected override async Task<FiniteElementsResponse> ProcessOperation(TRequest request)
        {
            var response = new FiniteElementsResponse();

            uint degreesOfFreedom = await this.CalculateDegreesFreedomMaximum(request.BeamData.NumberOfElements).ConfigureAwait(false);

            TBeam beam = await this.BuildBeam(request, degreesOfFreedom).ConfigureAwait(false);

            NewmarkMethodInput input = await this.CreateInput(beam, request, degreesOfFreedom).ConfigureAwait(false);

            while (input.AngularFrequency <= input.FinalAngularFrequency)
            {
                double time = input.InitialTime;
                input.TimeStep = await this._time.CalculateTimeStep(input.AngularFrequency, request.BeamData.PeriodDivision).ConfigureAwait(false);
                input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.BeamData.PeriodCount).ConfigureAwait(false);

                string path = await this.CreatePath(request, input, response).ConfigureAwait(false);

                var previousResult = new AnalysisResult
                {
                    Displacement = new double[input.NumberOfTrueBoundaryConditions],
                    Velocity = new double[input.NumberOfTrueBoundaryConditions],
                    Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                    Force = input.OriginalForce
                };

                while (time <= input.FinalTime)
                {
                    AnalysisResult result;

                    if (time == input.InitialTime)
                    {
                        //result = await this._numericalMethod.CalculateResultForInitialTime(input).ConfigureAwait(false);
                        result = previousResult;
                    }
                    else
                    {
                        //input.Force = input.OriginalForce.MultiplyEachElement(Math.Cos(input.AngularFrequency * time));
                        
                        result = await this._numericalMethod.CalculateResult(input, previousResult, time).ConfigureAwait(false);
                    }

                    this._auxiliarOperation.WriteInFile(time, result.Displacement, path);

                    previousResult = result;

                    time += input.TimeStep;
                }

                input.AngularFrequency += input.AngularFrequencyStep;
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

        /// <summary>
        /// Calculates the degrees freedom maximum.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public Task<uint> CalculateDegreesFreedomMaximum(uint numberOfElements)
        {
            return Task.FromResult((numberOfElements + 1) * Constant.NodesPerElement);
        }
    }
}
