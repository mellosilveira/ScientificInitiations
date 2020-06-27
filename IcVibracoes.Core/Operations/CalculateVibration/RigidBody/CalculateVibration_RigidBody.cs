using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Operations.CalculateVibration;
using IcVibracoes.DataContracts.RigidBody;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It's responsible to calculate the vibration for a rigid body analysis.
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

        protected override async Task<TResponse> ProcessOperation(TRequest request)
        {
            var response = new TResponse();

            base._numericalMethod = NumericalMethodFactory.CreateMethod(request.NumericalMethod);

            double[] initial_y = await this.BuildInitialConditions(request).ConfigureAwait(false);

            TInput input = await this.CreateInput(request).ConfigureAwait(false);

            foreach (double dampingRatio in request.DampingRatioList)
            {
                input.DampingRatio = dampingRatio;

                string maxValuesPath = await this.CreateMaxValuesPath(request, input).ConfigureAwait(false);

                while (input.AngularFrequency <= input.FinalAngularFrequency)
                {
                    input.TimeStep = await this._time.CalculateTimeStep(input.Mass, input.Stiffness, input.AngularFrequency, request.PeriodDivision).ConfigureAwait(false);
                    input.FinalTime = await this._time.CalculateFinalTime(input.AngularFrequency, request.PeriodCount).ConfigureAwait(false);

                    double[] maxValues = new double[initial_y.Length];

                    string path = await this.CreateSolutionPath(request, input).ConfigureAwait(false);

                    if (path == null)
                    {
                        return response;
                    }

                    double time = 0;
                    double[] y = initial_y;

                    this._file.Write(time, y, path);

                    while (time <= input.FinalTime)
                    {
                        y = await this.CalculateRigidBodyResult(input, time, y).ConfigureAwait(false);

                        await this.CompareValuesAndUpdateMaxValuesResult(y, maxValues).ConfigureAwait(false);

                        this._file.Write(time + input.TimeStep, y, path);

                        time += input.TimeStep;
                    }

                    this._file.Write(input.AngularFrequency, maxValues, maxValuesPath);

                    input.AngularFrequency += input.AngularFrequencyStep;
                }
            }

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
                if (Math.Abs(result[i]) > Math.Abs(maxValuesResult[i]))
                {
                    maxValuesResult[i] = result[i];
                }
            }

            return Task.CompletedTask;
        }

        protected override Task<TResponse> ValidateOperation(TRequest request)
        {
            var response = new TResponse();

            return Task.FromResult(response);
        }
    }
}
