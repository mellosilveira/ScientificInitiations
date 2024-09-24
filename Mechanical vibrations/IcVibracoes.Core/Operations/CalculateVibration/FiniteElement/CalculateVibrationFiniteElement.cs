using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
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
    public abstract class CalculateVibrationFiniteElement<TRequest, TProfile, TBeam> : CalculateVibration<TRequest, FiniteElementResponse, FiniteElementResponseData, FiniteElementMethodInput>, ICalculateVibration_FiniteElement<TRequest, TProfile, TBeam>
        where TRequest : FiniteElementRequest<TProfile>
        where TProfile : Profile, new()
        where TBeam : IBeam<TProfile>, new()
    {
        private readonly IProfileValidator<TProfile> _profileValidator;
        private readonly ITime _time;
        private readonly INaturalFrequency _naturalFrequency;
        private readonly IMainMatrix<TBeam, TProfile> _mainMatrix;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        /// <param name="mainMatrix"></param>
        protected CalculateVibrationFiniteElement(
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency,
            IMainMatrix<TBeam, TProfile> mainMatrix)
        {
            this._profileValidator = profileValidator;
            this._time = time;
            this._naturalFrequency = naturalFrequency;
            this._mainMatrix = mainMatrix;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="TBeam"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>A new instance of class <see cref="TBeam"/>.</returns>
        public abstract TBeam BuildBeam(TRequest request, uint degreesOfFreedom);

        /// <summary>
        /// This method creates the input to be used in finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new instance of class <see cref="FiniteElementMethodInput"/>.</returns>
        public override async Task<FiniteElementMethodInput> CreateInputAsync(TRequest request)
        {
            uint degreesOfFreedom = CalculateDegreesOfFreedom(request.NumberOfElements);

            TBeam beam = this.BuildBeam(request, degreesOfFreedom);

            (bool[] boundaryConditions, uint numberOfTrueBoundaryConditions) = await this._mainMatrix.CalculateBoundaryConditionsAsync(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] mass = await this._mainMatrix.CalculateMassAsync(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] stiffness = await this._mainMatrix.CalculateStiffnessAsync(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] damping = await this._mainMatrix.CalculateDamping(mass, stiffness).ConfigureAwait(false);

            double[] forces = await this._mainMatrix.CalculateForce(beam).ConfigureAwait(false);
            
            FiniteElementMethodInput input = await base.CreateInputAsync(request).ConfigureAwait(false);
            input.NumericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true);
            input.Mass = await mass.ApplyBoundaryConditionsAsync(boundaryConditions, numberOfTrueBoundaryConditions).ConfigureAwait(false);
            input.Stiffness = await stiffness.ApplyBoundaryConditionsAsync(boundaryConditions, numberOfTrueBoundaryConditions).ConfigureAwait(false);
            input.Damping = await damping.ApplyBoundaryConditionsAsync(boundaryConditions, numberOfTrueBoundaryConditions).ConfigureAwait(false);
            input.OriginalForce = await forces.ApplyBoundaryConditionsAsync(boundaryConditions, numberOfTrueBoundaryConditions).ConfigureAwait(false);
            input.NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions;

            return input;
        }

        /// <summary>
        /// This method calculates the vibration using finite element concepts and writes the results in a file.
        /// Each line in the file contains the result in an instant of time at an angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ProcessOperationAsync(TRequest request)
        {
            await Task.Delay(0);

            var response = new FiniteElementResponse { Data = new FiniteElementResponseData() };
            response.SetSuccessCreated();

            // Step 1 - Sets the numerical method to be used in analysis.
            base.NumericalMethod = NumericalMethodFactory.CreateMethod(request.NumericalMethod);

            // Step 2 - Creates the input to numerical method.
            FiniteElementMethodInput input = await this.CreateInputAsync(request).ConfigureAwait(false);

            // Step 3 - Generates the path to save the maximum values of analysis results and save the file URI.
            string maxValuesPath = this.CreateMaxValuesPath(request, input);

            ICollection<string> fileUris = new Collection<string>();
            fileUris.Add(Path.GetDirectoryName(maxValuesPath));

            while (input.AngularFrequency <= input.FinalAngularFrequency)
            {
                // Step 4 - Sets the value for time step and final time based on angular frequency and element mechanical properties.
                input.TimeStep = this._time.CalculateTimeStep(input.AngularFrequency, request.PeriodDivision);
                input.FinalTime = this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount);

                // Step 5 - Generates the path to save the analysis results.
                // Each combination of damping ratio and angular frequency will have a specific path.
                string solutionPath = this.CreateSolutionPath(request, input);

                string solutionFolder = Path.GetDirectoryName(solutionPath);

                if (fileUris.Contains(solutionFolder) == false)
                {
                    fileUris.Add(Path.GetDirectoryName(solutionPath));
                }

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
                        FiniteElementResult result = this.NumericalMethod.CalculateFiniteElementResultForInitialTime(input);
                        streamWriter.WriteResult(input.InitialTime, result.Displacement);

                        // Step 7 - Sets the next time.
                        double time = input.InitialTime + input.TimeStep;

                        while (time <= input.FinalTime)
                        {
                            // Step 8 - Calculates the results and writes it into a file.
                            result = this.NumericalMethod.CalculateFiniteElementResult(input, previousResult, time);
                            streamWriter.WriteResult(time, result.Displacement);

                            previousResult = result;

                            // Step 9 - Compares the previous results with the new calculated to catch the maximum values.
                            CompareValuesAndUpdateMaxValuesResult(result, maxValuesResult);

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
            //double[] naturalFrequencies = this._naturalFrequency.CalculateByQRDecomposition(input.Mass, input.Stiffness, tolerance: 1e-3);
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
        private static uint CalculateDegreesOfFreedom(uint numberOfElements) => (numberOfElements + 1) * Constants.NodesPerElement;

        /// <summary>
        /// This method compares the values and update the result with maximum values.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="maxValuesResult"></param>
        /// <returns></returns>
        private static void CompareValuesAndUpdateMaxValuesResult(FiniteElementResult result, FiniteElementResult maxValuesResult)
        {
            for (uint i = 0; i < result.Displacement.Length; i++)
            {
                var displacement = Math.Abs(result.Displacement[i]);
                if (Math.Abs(maxValuesResult.Displacement[i]) < displacement)
                {
                    maxValuesResult.Displacement[i] = displacement;
                }

                var velocity = Math.Abs(result.Velocity[i]);
                if (Math.Abs(maxValuesResult.Velocity[i]) < velocity)
                {
                    maxValuesResult.Velocity[i] = velocity;
                }

                var acceleration = Math.Abs(result.Acceleration[i]);
                if (Math.Abs(maxValuesResult.Acceleration[i]) < acceleration)
                {
                    maxValuesResult.Acceleration[i] = acceleration;
                }

                var force = Math.Abs(result.Force[i]);
                if (Math.Abs(maxValuesResult.Force[i]) < force)
                {
                    maxValuesResult.Force[i] = force;
                }
            }
        }

        /// <summary>
        /// This method validates the finite element request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperationAsync(TRequest request)
        {
            var response = await base.ValidateOperationAsync(request).ConfigureAwait(false);
            if (response.Success == false)
            {
                return response;
            }

            response
                .AddErrorIf(() => request.NumberOfElements <= 0, "Number of elements must be greater than zero.")
                .AddErrorIf(() => Enum.TryParse(typeof(MaterialType), request.Material, out _) == false, $"Invalid material: '{request.Material}'.")
                .AddErrorIf(() => request.Length <= 0, $"Length: '{request.Length}' must be greater than zero.")
                .AddErrorIf(() => this._profileValidator.Execute(request.Profile, response) == false, "Invalid beam profile.")
                .AddErrorIf(request.Forces, force => force.NodePosition < 0 || force.NodePosition > request.NumberOfElements, force => $"Invalid value for fastening node position: {force.NodePosition}. It must be greater than zero and less than {request.NumberOfElements}.");

            foreach (var fastening in request.Fastenings)
            {
                response.AddErrorIf(() => fastening.NodePosition < 0 || fastening.NodePosition > request.NumberOfElements, $"Invalid value for fastening node position: '{fastening.NodePosition}'. It must be greater than zero and less than '{request.NumberOfElements}'.");
                response.AddErrorIf(() => Enum.TryParse(typeof(Fastenings), fastening.Type, out _) == false, $"Invalid fastening type: '{fastening.Type}'.");
            }

            return response;
        }
    }
}
