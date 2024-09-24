﻿using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.Calculator.Force;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder;
using System;

namespace IcVibracoes.Core.Models
{
    /// <summary>
    /// The numerical methods availables.
    /// The explanation for each numerical method can be found in the file "Numerical Integration.pdf", in the folder "Bibliography".
    /// </summary>
    public enum NumericalMethod
    {
        CentralDifference = 1,
        ImplicitLinearAcceleration = 2,
        NewmarkBeta = 3,
        Newmark = 4,
        RungeKuttaForthOrder = 5 }

    /// <summary>
    /// It's responsible to manipulate the enum <see cref="NumericalMethod"/> based in a string.
    /// </summary>
    public class NumericalMethodFactory
    {
        /// <summary>
        /// This method creates an instance of interface <seealso cref="INumericalIntegrationMethod"/>.
        /// It can be <seealso cref="NewmarkBetaMethod"/> (used in <see cref="NumericalMethod.CentralDifference"/>, <see cref="NumericalMethod.ImplicitLinearAcceleration"/> and <see cref="NumericalMethod.NewmarkBeta"/>) or <seealso cref="NewmarkMethod"/>.
        /// </summary>
        /// <param name="numericalMethod"></param>
        /// <returns></returns>
        public static INumericalIntegrationMethod CreateMethod(string numericalMethod)
        {
            switch ((NumericalMethod)Enum.Parse(typeof(NumericalMethod), numericalMethod, ignoreCase: true))
            {
                case NumericalMethod.CentralDifference:
                case NumericalMethod.ImplicitLinearAcceleration:
                case NumericalMethod.NewmarkBeta:
                    return new NewmarkBetaMethod(new Force(), new MappingResolver());
                case NumericalMethod.Newmark:
                    return new NewmarkMethod(new MappingResolver());
                case NumericalMethod.RungeKuttaForthOrder:
                    return new RungeKuttaForthOrderMethod(new DifferentialEquationOfMotion(new Force()), new MappingResolver());
            }

            throw new Exception($"Invalid numerical method: '{numericalMethod}'.");
        }
    }
}
