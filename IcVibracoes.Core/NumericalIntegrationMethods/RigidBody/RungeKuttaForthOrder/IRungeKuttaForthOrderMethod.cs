using IcVibracoes.Core.DTO.InputData;
using IcVibracoes.DataContracts.RigidBody;
using System.Threading.Tasks;

namespace IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder
{
    /// <summary>
    /// It is responsible to execute the Runge Kutta Forth Order numerical integration method to calculate vibration.
    /// </summary>
    public interface IRungeKuttaForthOrderMethod<TRequest, TRequestData, TResponse, TResponseData>
        where TRequestData : RigidBodyRequestData
        where TRequest : RigidBodyRequest<TRequestData>
        where TResponseData : RigidBodyResponseData, new()
        where TResponse : RigidBodyResponse<TResponseData>, new()
    {
        /// <summary>
        /// Calculates the response of the Runge Kutta Forth Order numerical integration.
        /// </summary>
        /// <param name="timeStep"></param>
        /// <param name="time"></param>
        /// <param name="y"></param>
        /// <param name="angularFrequency"></param>
        /// <returns></returns>
        Task<double[]> ExecuteMethod(DifferentialEquationOfMotionInput input, double timeStep, double time, double[] y);
    }
}
