using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.NumericalIntegrationMethods;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder;
using IcVibracoes.DataContracts;
using System;

namespace IcVibracoes.Core.Models
{
    /// <summary>
    /// The numerical methods availables.
    /// The explanation for each numerical method can be found in the file "Numerical Integration.pdf", in the folder "Bibliography".
    /// </summary>
    public enum NumericalMethod
    {
        CentralDifferenceMethod = 1,

        ImplicitLinearAccelerationMethod = 2,

        NewmarkBeta = 3,

        Newmark = 4,

        RungeKuttaForthOrder = 5
    }

    /// <summary>
    /// It's responsible to create a force type object based on a string.
    /// </summary>
    public class NumericalMethodFactory
    {
        /// <summary>
        /// Creates a new instance of a NumericalMethod enum.
        /// </summary>
        /// <param name="numericalMethod"></param>
        /// <returns></returns>
        public static NumericalMethod Create(string numericalMethod)
        {
            switch ((NumericalMethod)Enum.Parse(typeof(NumericalMethod), numericalMethod, ignoreCase: true))
            {
                case NumericalMethod.CentralDifferenceMethod:
                    return NumericalMethod.CentralDifferenceMethod;
                case NumericalMethod.ImplicitLinearAccelerationMethod:
                    return NumericalMethod.ImplicitLinearAccelerationMethod;
                case NumericalMethod.NewmarkBeta:
                    return NumericalMethod.NewmarkBeta;
                case NumericalMethod.Newmark:
                    return NumericalMethod.Newmark;
                case NumericalMethod.RungeKuttaForthOrder:
                    return NumericalMethod.RungeKuttaForthOrder;
                default:
                    return default;
            }
        }

        /// <summary>
        /// Creates a new instance of NumericalIntegrationMethod class.
        /// </summary>
        /// <param name="numericalMethod"></param>
        /// <returns></returns>
        public static INumericalIntegrationMethod CreateMethod(string numericalMethod)
        {
            switch ((NumericalMethod)Enum.Parse(typeof(NumericalMethod), numericalMethod, ignoreCase: true))
            {
                case NumericalMethod.CentralDifferenceMethod:
                    return new NewmarkBetaMethod(new ArrayOperation());
                case NumericalMethod.ImplicitLinearAccelerationMethod:
                    return new NewmarkBetaMethod(new ArrayOperation());
                case NumericalMethod.NewmarkBeta:
                    return new NewmarkBetaMethod(new ArrayOperation());
                case NumericalMethod.Newmark:
                    return new NewmarkMethod(new ArrayOperation());
                case NumericalMethod.RungeKuttaForthOrder:
                    return null;
                default:
                    return default;
            }
        }

        /// <summary>
        /// Validates the numerical method passed in request.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="numericalMethod"></param>
        /// <param name="response"></param>
        public static void Validate<TResponse, TResponseData>(string numericalMethod, TResponse response)
            where TResponse : OperationResponseBase<TResponseData>
            where TResponseData : OperationResponseData
        {
            if (Enum.TryParse(typeof(NumericalMethod), numericalMethod, ignoreCase: true, out object _) == false)
            {
                response.AddError("", $"Invalid numerical method: '{numericalMethod}'.");
            }
        }
    }
}
