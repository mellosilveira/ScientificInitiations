using IcVibracoes.Core.DTO;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom
{
    /// <summary>
    /// It is responsible to calculate the vibration for a rigid body with one degrees freedom case.
    /// </summary>
    public class CalculateVibrationToOneDegreeFreedom : CalculateVibration_RigidBody<OneDegreeFreedomRequest, OneDegreeFreedomRequestData, OneDegreeFreedomResponse, OneDegreeFreedomResponseData>, ICalculateVibrationToOneDegreeFreedom
    {
        private readonly IMappingResolver _mappingResolver;
        private readonly IRungeKuttaForthOrderMethod_1DF _rungeKutta;

        public CalculateVibrationToOneDegreeFreedom(
            IMappingResolver mappingResolver, 
            IRungeKuttaForthOrderMethod_1DF rungeKutta)
        {
            this._mappingResolver = mappingResolver;
            this._rungeKutta = rungeKutta;
        }

        public override Task<double[]> CalculateDifferencialEquationOfMotion(DifferentialEquationOfMotionInput input, double time, double[] y)
        {
            double[] result = new double[Constant.NumberOfRigidBodyVariables_1DF];

            // wn - Natural angular frequency
            double wn = Math.Sqrt(input.Hardness / input.Mass);
            double damping = input.DampingRatio * 2 * input.Mass * wn;

            // Velocity of primary object.
            result[0] = y[1];
            // Acceleration of primary object.
            result[1] = (input.Force * Math.Sin(input.AngularFrequency * time) - (damping * y[1]) - (input.Hardness * y[0])) / input.Mass;

            return Task.FromResult(result);
        }

        protected override async Task<OneDegreeFreedomResponse> ProcessOperation(OneDegreeFreedomRequest request)
        {
            var response = new OneDegreeFreedomResponse();

            double time = request.Data.InitialTime;
            double timeStep = request.Data.TimeStep;
            double finalTime = request.Data.FinalTime;

            DifferentialEquationOfMotionInput input = await this._mappingResolver.BuildFrom(request.Data);

            // Parallel.Foreach
            foreach (var dampingRatio in request.Data.DampingRatioList)
            {
                while (time <= finalTime)
                {
                    double[] y = new double[Constant.NumberOfRigidBodyVariables_1DF]
                    {
                        request.Data.InitialDisplacement,
                        request.Data.InitialVelocity
                    };

                    y = await this._rungeKutta.ExecuteMethod(input, timeStep, time, y);

                    

                    time += timeStep;
                }
            }

            return response;
        }

        protected override Task<OneDegreeFreedomResponse> ValidateOperation(OneDegreeFreedomRequest request)
        {
            var response = new OneDegreeFreedomResponse();

            return Task.FromResult(response);
        }
    }
}
