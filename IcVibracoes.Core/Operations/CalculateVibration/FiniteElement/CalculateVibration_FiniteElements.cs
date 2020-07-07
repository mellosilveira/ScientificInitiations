using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement
{
    /// <summary>
    /// It's responsible to calculate the beam vibration using finite element concepts.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TProfile"></typeparam>
    /// <typeparam name="TBeam"></typeparam>
    public abstract class CalculateVibration_FiniteElement<TRequest, TProfile, TBeam> : CalculateVibration<TRequest, FiniteElementResponse, FiniteElementResponseData, FiniteElementMethodInput>, ICalculateVibration_FiniteElement<TRequest, TProfile, TBeam>
        where TRequest : FiniteElementRequest<TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        private readonly IProfileValidator<TProfile> _profileValidator;
        private readonly ITime _time;
        private readonly INaturalFrequency _naturalFrequency;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateVibration_FiniteElement(
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
        {
            this._profileValidator = profileValidator;
            this._time = time;
            this._naturalFrequency = naturalFrequency;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="TBeam"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="TBeam"/>.</returns>
        public abstract Task<TBeam> BuildBeam(TRequest request, uint degreesOfFreedom, FiniteElementResponse response);

        /// <summary>
        /// This method calculates the vibration using finite element concepts and writes the results in a file.
        /// Each line in the file contains the result in an instant of time at an angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ProcessOperation(TRequest request)
        {
            var response = new FiniteElementResponse { Data = new FiniteElementResponseData() };
            response.SetSuccessCreated();

            // Step 1 - Sets the numerical method to be used in analysis.
            base._numericalMethod = NumericalMethodFactory.CreateMethod(request.NumericalMethod, response);

            // Step 2 - Creates the input to numerical method.
            FiniteElementMethodInput input = await this.CreateInput(request, response).ConfigureAwait(false);

            // Step 3 - Generates the path to save the maximum values of analysis results and save the file URI.
            string maxValuesPath = await this.CreateMaxValuesPath(request, input).ConfigureAwait(false);

            ICollection<string> fileUris = new Collection<string>();
            fileUris.Add(Path.GetDirectoryName(maxValuesPath));

            while (input.AngularFrequency <= input.FinalAngularFrequency)
            {
                // Step 4 - Sets the value for time step and final time based on angular frequency and element mechanical properties.
                input.TimeStep = await this._time.CalculateTimeStep(input.AngularFrequency, request.PeriodDivision).ConfigureAwait(false);
                input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount).ConfigureAwait(false);

                // Step 5 - Generates the path to save the analysis results.
                // Each combination of damping ratio and angular frequency will have a specific path.
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

                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(solutionPath))
                    {
                        // Step 6 - Calculates the initial results and writes it into a file.
                        FiniteElementResult result = await this._numericalMethod.CalculateFiniteElementResultForInitialTime(input).ConfigureAwait(false);
                        streamWriter.WriteResult(input.InitialTime, result.Displacement);

                        // Step 7 - Sets the next time.
                        double time = input.InitialTime + input.TimeStep;

                        while (time <= input.FinalTime)
                        {
                            // Step 8 - Calculates the results and writes it into a file.
                            result = await this._numericalMethod.CalculateFiniteElementResult(input, previousResult, time).ConfigureAwait(false);
                            streamWriter.WriteResult(time, result.Displacement);

                            previousResult = result;

                            // Step 9 - Compares the previous results with the new calculated to catch the maximum values.
                            await this.CompareValuesAndUpdateMaxValuesResult(result, maxValuesResult).ConfigureAwait(false);

                            time += input.TimeStep;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.AddError(OperationErrorCode.InternalServerError, $"Occurred error while calculating the analysis results and writing them in the file. {ex.Message}", HttpStatusCode.InternalServerError);

                    response.SetInternalServerError();
                    return response;
                }

                try
                {
                    // Step 10 - Writes the maximum values of analysis result into a file.
                    using (StreamWriter streamWriter = new StreamWriter(maxValuesPath, true))
                    {
                        streamWriter.WriteResult(input.AngularFrequency, maxValuesResult.Displacement);
                    }
                }
                catch (Exception ex)
                {
                    response.AddError(OperationErrorCode.InternalServerError, $"Occurred error while writing the maximum values in the file. {ex.Message}", HttpStatusCode.InternalServerError);

                    response.SetInternalServerError();
                    return response;
                }

                input.AngularFrequency += input.AngularFrequencyStep;
            }

            // Step 11 - Calculates the structure natural frequencies and writes them into a file.
            //double[] naturalFrequencies = await this._naturalFrequency.CalculateByQRDecomposition(input.Mass, input.Stiffness, tolerance: 1e-3).ConfigureAwait(false);
            //this._file.Write("Natural Frequencies", naturalFrequencies, maxValuesPath);

            // Step 12 - Maps the response.
            response.Data.Author = request.Author;
            response.Data.AnalysisExplanation = "Not implemented.";
            response.Data.FileUris = fileUris;
            return response;
        }

        /// <summary>
        /// This method calculates the degrees of freedom maximum.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        protected Task<uint> CalculateDegreesOfFreedomMaximum(uint numberOfElements)
        {
            return Task.FromResult((numberOfElements + 1) * Constant.NodesPerElement);
        }

        /// <summary>
        /// This method compares the values and update the result with maximum values.
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
        /// This method validates the finite element request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperation(TRequest request)
        {
            FiniteElementResponse response = await base.ValidateOperation(request).ConfigureAwait(false);

            if (response.Success == false)
            {
                return response;
            }

            if (request.NumberOfElements <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, "Number of elements must be greather than zero.");
            }

            if (Enum.TryParse(typeof(Materials), request.Material, out _) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid material: '{request.Material}'.");
            }

            if (request.Length <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Length: '{request.Length}' must be greather than zero.");
            }

            foreach (var fastening in request.Fastenings)
            {
                if (fastening.NodePosition < 0 || fastening.NodePosition > request.NumberOfElements)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Invalid value for fastening node position: '{fastening.NodePosition}'. It must be greather than zero and less than '{request.NumberOfElements}'.");
                }

                if (Enum.TryParse(typeof(Fastenings), fastening.Type, out _) == false)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Invalid fastening type: '{fastening.Type}'.");
                }
            }

            foreach (var forces in request.Forces)
            {
                if (forces.NodePosition < 0 || forces.NodePosition > request.NumberOfElements)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Invalid value for fastening node position: {forces.NodePosition}. It must be greather than zero and less than {request.NumberOfElements}.");
                }
            }

            await this._profileValidator.Execute(request.Profile, response).ConfigureAwait(false);

            return response;
        }
    }
}
