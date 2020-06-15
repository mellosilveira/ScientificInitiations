using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.DataContracts.FiniteElements;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements
{
    /// <summary>
    /// It's responsible to calculate the beam vibration for finite element analysis.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeam"></typeparam>
    public abstract class CalculateVibration_FiniteElements<TRequest, TProfile, TBeam, TInput> : CalculateVibration<TRequest, FiniteElementsResponse, FiniteElementsResponseData, TInput>, ICalculateVibration_FiniteElements<TRequest, TProfile, TBeam, TInput>
        where TRequest : FiniteElementsRequest<TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
        where TInput : NewmarkMethodInput, new()
    {
        private readonly IFile _file;
        private readonly ITime _time;
        private readonly INewmarkMethod _numericalMethod;
        private readonly INaturalFrequency _naturalFrequency;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="time"></param>
        /// <param name="newmarkMethod"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateVibration_FiniteElements(
            IFile file,
            ITime time,
            INewmarkMethod newmarkMethod,
            INaturalFrequency naturalFrequency)
        {
            this._file = file;
            this._time = time;
            this._numericalMethod = newmarkMethod;
            this._naturalFrequency = naturalFrequency;
        }

        /// <summary>
        /// Builds the beam.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TBeam> BuildBeam(TRequest request, uint degreesOfFreedom);

        protected override async Task<FiniteElementsResponse> ProcessOperation(TRequest request)
        {
            var response = new FiniteElementsResponse();

            TInput input = await this.CreateInput(request).ConfigureAwait(false);

            string maxValuesPath = await this.CreateMaxValuesPath(request, input, response).ConfigureAwait(false);

            while (input.AngularFrequency <= input.FinalAngularFrequency)
            {
                double time = input.InitialTime;
                input.TimeStep = await this._time.CalculateTimeStep(input.AngularFrequency, request.PeriodDivision).ConfigureAwait(false);
                input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount).ConfigureAwait(false);

                string solutionPath = await this.CreateSolutionPath(request, input, response).ConfigureAwait(false);

                var previousResult = new FiniteElementResult
                {
                    Displacement = new double[input.NumberOfTrueBoundaryConditions],
                    Velocity = new double[input.NumberOfTrueBoundaryConditions],
                    Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                    Force = input.OriginalForce
                };

                var maxValuesResult = new FiniteElementResult
                {
                    Displacement = new double[input.NumberOfTrueBoundaryConditions],
                    Velocity = new double[input.NumberOfTrueBoundaryConditions],
                    Acceleration = new double[input.NumberOfTrueBoundaryConditions],
                    Force = new double[input.NumberOfTrueBoundaryConditions]
                };

                while (time <= input.FinalTime)
                {
                    FiniteElementResult result;

                    if (time == input.InitialTime)
                    {
                        result = await this._numericalMethod.CalculateResultForInitialTime(input).ConfigureAwait(false);
                    }
                    else
                    {
                        result = await this._numericalMethod.CalculateResult(input, previousResult, time).ConfigureAwait(false);
                    }

                    this._file.Write(time, result.Displacement, solutionPath);

                    previousResult = result;
                    maxValuesResult = await this.CompareValues(result, maxValuesResult, input.NumberOfTrueBoundaryConditions).ConfigureAwait(false);

                    time += input.TimeStep;
                }

                this._file.Write(input.AngularFrequency, maxValuesResult.Displacement, maxValuesPath);

                input.AngularFrequency += input.AngularFrequencyStep;
            }

            double[] naturalFrequencies = await this._naturalFrequency.CalculateByQRDecomposition(input.Mass, input.Stiffness, tolerance: 1e-3).ConfigureAwait(false);
            this._file.Write("Natural Frequencies", naturalFrequencies, maxValuesPath);

            return response;
        }

        /// <summary>
        /// Calculates the degrees freedom maximum.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        protected Task<uint> CalculateDegreesFreedomMaximum(uint numberOfElements)
        {
            return Task.FromResult((numberOfElements + 1) * Constant.NodesPerElement);
        }

        /// <summary>
        /// Compares the values
        /// </summary>
        /// <param name="result"></param>
        /// <param name="maxValuesResult"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private Task<FiniteElementResult> CompareValues(FiniteElementResult result, FiniteElementResult maxValuesResult, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                if (maxValuesResult.Displacement[i] < Math.Abs(result.Displacement[i]))
                {
                    maxValuesResult.Displacement[i] = Math.Abs(result.Displacement[i]);
                }

                if (maxValuesResult.Velocity[i] < Math.Abs(result.Velocity[i]))
                {
                    maxValuesResult.Velocity[i] = Math.Abs(result.Velocity[i]);
                }

                if (maxValuesResult.Acceleration[i] < Math.Abs(result.Acceleration[i]))
                {
                    maxValuesResult.Acceleration[i] = Math.Abs(result.Acceleration[i]);
                }

                if (maxValuesResult.Force[i] < Math.Abs(result.Force[i]))
                {
                    maxValuesResult.Force[i] = Math.Abs(result.Force[i]);
                }
            }

            return Task.FromResult(maxValuesResult);
        }

        /// <summary>
        /// It's responsible to validade
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async override Task<FiniteElementsResponse> ValidateOperation(TRequest request)
        {
            FiniteElementsResponse response = new FiniteElementsResponse();

            return response;
        }
    }
}
