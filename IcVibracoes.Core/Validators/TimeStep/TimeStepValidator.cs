using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.DataContracts;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.TimeStep
{
    /// <summary>
    /// It's responsible to validate the time step for each integration method.
    /// </summary>
    public class TimeStepValidator : ITimeStepValidator
    {
        private readonly INaturalFrequency _naturalFrequency;

        public TimeStepValidator(
            INaturalFrequency naturalFrequency)
        {
            this._naturalFrequency = naturalFrequency;
        }

        /// <summary>
        /// Validates the time step of Runge Kutta numerical integration method.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="response"></param>
        /// <param name="mass"></param>
        /// <param name="stiffness"></param>
        /// <param name="timeStep"></param>
        /// <returns></returns>
        public async Task<bool> RungeKutta<TResponse, TResponseData>(TResponse response, double mass, double stiffness, double timeStep)
            where TResponseData : OperationResponseData
            where TResponse : OperationResponseBase<TResponseData>
        {
            double naturalFrequency = await this._naturalFrequency.Calculate(mass, stiffness).ConfigureAwait(false);

            double naturalPeriod = 2 * Math.PI / naturalFrequency;

            if (timeStep > naturalPeriod / 10)
            {
                response.AddError("", $"Time step: {timeStep} must be less than one-tenth of natural periodo: {naturalPeriod}.");

                return false;
            }

            return true;
        }
    }
}
