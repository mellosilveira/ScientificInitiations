using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.DataContracts;
using System;

namespace IcVibracoes.Core.Validators.TimeStep
{
    /// <summary>
    /// It's responsible to validate the time step for each integration method.
    /// </summary>
    public class TimeStepValidator : ITimeStepValidator
    {
        private readonly INaturalFrequency _naturalFrequency;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="naturalFrequency"></param>
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
        public bool RungeKutta<TResponse, TResponseData>(TResponse response, double mass, double stiffness, double timeStep)
            where TResponseData : OperationResponseData
            where TResponse : OperationResponseBase<TResponseData>
        {
            double naturalFrequency = this._naturalFrequency.Calculate(mass, stiffness);

            double naturalPeriod = 2 * Math.PI / naturalFrequency;

            if (timeStep > naturalPeriod / 10)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Time step: {timeStep} must be less than one-tenth of natural period: {naturalPeriod}.");

                return false;
            }

            return true;
        }
    }
}
