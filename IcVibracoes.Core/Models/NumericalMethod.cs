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
