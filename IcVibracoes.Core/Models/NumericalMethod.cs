using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.NumericalIntegrationMethods;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta;
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
    /// It's responsible to manipulate the enum <see cref="NumericalMethod"/> based in a string.
    /// </summary>
    public class NumericalMethodFactory
    {
        /// <summary>
        /// This method creates an instance of interface <seealso cref="INumericalIntegrationMethod"/>.
        /// It can be <seealso cref="NewmarkBetaMethod"/> (used in <see cref="NumericalMethod.CentralDifferenceMethod"/>, <see cref="NumericalMethod.ImplicitLinearAccelerationMethod"/> and <see cref="NumericalMethod.NewmarkBeta"/>), <seealso cref="NewmarkMethod"/> or <seealso cref="Pinned"/>.
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="numericalMethod"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static INumericalIntegrationMethod CreateMethod<TResponseData>(string numericalMethod, OperationResponseBase<TResponseData> response)
            where TResponseData : OperationResponseData
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
                    throw new NotImplementedException("Cannot use RungeKuttaForthOrder numerical method in this context.");
                default:
                    break;
            }

            response.AddError(OperationErrorCode.InternalServerError, $"Invalid numerical method: '{numericalMethod}'.");
            return null;
        }
    }
}
