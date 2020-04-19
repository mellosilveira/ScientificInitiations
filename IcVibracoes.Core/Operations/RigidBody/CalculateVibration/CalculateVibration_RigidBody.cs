﻿using IcVibracoes.Core.DTO;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body analysis.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TRequestData"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseData"></typeparam>
    public abstract class CalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData> : OperationBase<TRequest, TResponse, TResponseData>, ICalculateVibration_RigidBody<TRequest, TRequestData, TResponse, TResponseData>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData
        where TResponse : RigidBodyResponse<TResponseData>, new()
    {
        /// <summary>
        /// Calculates the value of the differential equation of motion for a specific time, based on the force and angular frequency that are passed.
        /// For each case, with one or two degrees of freedom, there is a different differential equation of motion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <param name="angularFrequency"></param>
        /// <returns></returns>
        public abstract Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y);

        protected override Task<TResponse> ProcessOperation(TRequest request)
        {
            var result = new TResponse();

            // Chamar runge-kutta
            // Runge-kutta usará a equação de movimento calculada nesta classe.

            return Task.FromResult(result);
        }

        protected override Task<TResponse> ValidateOperation(TRequest request)
        {
            var result = new TResponse();

            return Task.FromResult(result);
        }
    }
}
