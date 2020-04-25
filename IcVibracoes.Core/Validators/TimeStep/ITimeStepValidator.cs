using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Validators.TimeStep
{
    /// <summary>
    /// It's responsible to validate the time step for each integration method.
    /// </summary>
    public interface ITimeStepValidator
    {
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
        Task<bool> RungeKutta<TResponse, TResponseData>(TResponse response, double mass, double stiffness, double timeStep)
            where TResponse : OperationResponseBase<TResponseData>
            where TResponseData : OperationResponseData;
    }
}
