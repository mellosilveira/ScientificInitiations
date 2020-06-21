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
    public abstract class CalculateVibration_FiniteElements<TRequest, TProfile, TBeam> : CalculateVibration<TRequest, FiniteElementsResponse, FiniteElementsResponseData, FiniteElementsMethodInput>, ICalculateVibration_FiniteElements<TRequest, TProfile, TBeam>
        where TRequest : FiniteElementsRequest<TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
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

            FiniteElementsMethodInput input = await this.CreateInput(request).ConfigureAwait(false);

            string maxValuesPath = await this.CreateMaxValuesPath(request, input).ConfigureAwait(false);

            while (input.AngularFrequency <= input.FinalAngularFrequency)
            {
                double time = input.InitialTime;
                input.TimeStep = await this._time.CalculateTimeStep(input.AngularFrequency, request.PeriodDivision).ConfigureAwait(false);
                input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount).ConfigureAwait(false);

                string solutionPath = await this.CreateSolutionPath(request, input).ConfigureAwait(false);

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
                    await this.CompareValuesAndUpdateMaxValuesResult(result, maxValuesResult).ConfigureAwait(false);

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
        /// Compares the values and update the result with max values.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="maxValuesResult"></param>
        /// <returns></returns>
        private Task CompareValuesAndUpdateMaxValuesResult(FiniteElementResult result, FiniteElementResult maxValuesResult)
        {
            int length = result.Displacement.Length;

            for (uint i = 0; i < length; i++)
            {
                if (Math.Abs(maxValuesResult.Displacement[i]) < Math.Abs(result.Displacement[i]))
                {
                    maxValuesResult.Displacement[i] = Math.Abs(result.Displacement[i]);
                }

                if (Math.Abs(maxValuesResult.Velocity[i]) < Math.Abs(result.Velocity[i]))
                {
                    maxValuesResult.Velocity[i] = Math.Abs(result.Velocity[i]);
                }

                if (Math.Abs(maxValuesResult.Acceleration[i]) < Math.Abs(result.Acceleration[i]))
                {
                    maxValuesResult.Acceleration[i] = Math.Abs(result.Acceleration[i]);
                }

                if (Math.Abs(maxValuesResult.Force[i]) < Math.Abs(result.Force[i]))
                {
                    maxValuesResult.Force[i] = Math.Abs(result.Force[i]);
                }
            }

            return Task.CompletedTask;
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
