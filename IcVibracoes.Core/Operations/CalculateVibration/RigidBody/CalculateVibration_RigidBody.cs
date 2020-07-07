using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Operations.CalculateVibration;
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.RigidBody;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration using rigid body concepts.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public abstract class CalculateVibration_RigidBody<TRequest, TResponse, TResponseData, TInput> : CalculateVibration<TRequest, TResponse, TResponseData, TInput>, ICalculateVibration_RigidBody<TRequest, TResponse, TResponseData, TInput>
        where TRequest : RigidBodyRequest
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
        where TInput : RigidBodyInput, new()
    {
        private readonly IFile _file;
        private readonly ITime _time;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="time"></param>
        public CalculateVibration_RigidBody(
            IFile file,
            ITime time)
        {
            this._file = file;
            this._time = time;
        }

        /// <summary>
        /// Calculates the result for rigid body analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract Task<double[]> CalculateRigidBodyResult(TInput input, double time, double[] y);

        /// <summary>
        /// Builds the vector with the initial conditions to analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<double[]> BuildInitialConditions(TRequest request);

        /// <summary>
        /// This method calculates the vibration using rigid body concepts and writes the results in a file.
        /// Each line in the file contains the result in an instant of time at an angular frequency and a damping ratio.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<TResponse> ProcessOperation(TRequest request)
        {
            var response = new TResponse();

            // Step 1 - Sets the numerical method to be used in analysis.
            base._numericalMethod = NumericalMethodFactory.CreateMethod(request.NumericalMethod, response);

            // Step 2 - Creates the input to numerical method.
            TInput input = await this.CreateInput(request, response).ConfigureAwait(false);

            // Step 3 - Creates the initial conditions for displacement and velocity.
            double[] initial_y = await this.BuildInitialConditions(request).ConfigureAwait(false);

            foreach (double dampingRatio in request.DampingRatios)
            {
                // Step 4 - Sets the initial angular frequency.
                input.AngularFrequency = request.InitialAngularFrequency;

                // Step 5 - Sets the damping ratio to be used in analysis.
                input.DampingRatio = dampingRatio;

                // Step 6 - Generates the path to save the maximum values of analysis results.
                string maxValuesPath = await this.CreateMaxValuesPath(request, input).ConfigureAwait(false);

                while (input.AngularFrequency <= input.FinalAngularFrequency)
                {
                    // Step 7 - Sets the value for time step and final time based on angular frequency and element mechanical properties.
                    input.TimeStep = await this._time.CalculateTimeStep(input.Mass, input.Stiffness, input.AngularFrequency, request.PeriodDivision).ConfigureAwait(false);
                    input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount).ConfigureAwait(false);

                    double[] maxValues = new double[initial_y.Length];

                    // Step 8 - Generates the path to save the analysis results.
                    // Each combination of damping ratio and angular frequency will have a specific path.
                    string path = await this.CreateSolutionPath(request, input).ConfigureAwait(false);

                    // Step 9 - Sets the initial conditions for time, displacement and velocity.
                    double time = 0;
                    double[] y = initial_y;

                    // Step 10 - Calculates the results and writes it into a file.
                    using (StreamWriter streamWriter = new StreamWriter(path))
                    {
                        //    // Step 10.1 - Writes the initial values into a file.
                        streamWriter.Write(string.Format("{0}; ", time + input.TimeStep));
                        for (int i = 0; i < y.Length; i++)
                        {
                            streamWriter.Write(string.Format("{0}; ", y[i]));
                        }
                        streamWriter.Write(streamWriter.NewLine);

                        while (time <= input.FinalTime)
                        {
                            // Step 10.2 - Calculates the analysis results.
                            y = await this.CalculateRigidBodyResult(input, time, y).ConfigureAwait(false);

                            // Step 10.3 - Writes the analysis results into a file.
                            streamWriter.Write(string.Format("{0}; ", time + input.TimeStep));
                            for (int i = 0; i < y.Length; i++)
                            {
                                streamWriter.Write(string.Format("{0}; ", y[i]));
                            }
                            streamWriter.Write(streamWriter.NewLine);

                            time += input.TimeStep;

                            // Step 11 - Compares the previous results with the new calculated.
                            await this.CompareValuesAndUpdateMaxValuesResult(y, maxValues).ConfigureAwait(false);
                        }
                    }

                    // Step 12 - Writes the maximum values of analysis result into a file.
                    using (StreamWriter streamWriter = new StreamWriter(maxValuesPath, true))
                    {
                        streamWriter.Write(string.Format("{0}; ", input.AngularFrequency));
                        for (int i = 0; i < maxValues.Length; i++)
                        {
                            streamWriter.Write(string.Format("{0}; ", maxValues[i]));
                        }
                        streamWriter.Write(streamWriter.NewLine);
                    }

                    input.AngularFrequency += input.AngularFrequencyStep;
                }
            }

            // Step 13 - Maps the response.
            // TODO: Adicionar as pastas no response.FileUris
            return response;
        }

        /// <summary>
        /// Compares the values and update the result with max values.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="maxValuesResult"></param>
        /// <returns></returns>
        private Task CompareValuesAndUpdateMaxValuesResult(double[] result, double[] maxValuesResult)
        {
            int length = result.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(maxValuesResult[i]) < Math.Abs(result[i]))
                {
                    maxValuesResult[i] = Math.Abs(result[i]);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// This method validates the rigid body request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<TResponse> ValidateOperation(TRequest request)
        {
            var response = await base.ValidateOperation(request).ConfigureAwait(false);

            if (response.Success == false)
            {
                return response;
            }

            if (request.DampingRatios.Any(v => v < 0))
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Damping ratio cannot be less than zero.");
            }

            return response;
        }
    }
}
