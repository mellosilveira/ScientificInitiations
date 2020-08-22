using IcVibracoes.Core.Models;
using System;

namespace IcVibracoes.Core.DTO.NumericalMethodInput
{
    /// <summary>
    /// It contains the input 'data' to all numerical methods.
    /// </summary>
    public class NumericalMethodInput
    {
        /// <summary>
        /// The initial time.
        /// Unit: s (second).
        /// </summary>
        public double InitialTime
        {
            get => 0;
        }

        /// <summary>
        /// The time step.
        /// Unit: s (second).
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// The final time.
        /// Unit: s (second).
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// The angular frequency.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double AngularFrequency { get; set; }

        /// <summary>
        /// The angular frequency step.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// The final angular frequency.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double FinalAngularFrequency { get; set; }

        /// <summary>
        /// The numerical method used in the finite element analysis.
        /// </summary>
        public NumericalMethod NumericalMethod { get; set; }

        /// <summary>
        /// Integration constant used in numerical method calculations.
        /// Valid values:
        /// 1. 0 --> Central difference method.
        /// 2. 1/4 --> Newmark method is implicit and unconditionally stable. In this case the acceleration within the time interval [ti, ti+1) is presumed to be constant.
        /// 3. 1/6 --> Linear acceleration method. In this case the acceleration within the time interval [ti, ti+1) is presumed to be linear.
        /// </summary>
        public double Beta
        {
            get
            {
                switch (this.NumericalMethod)
                {
                    case NumericalMethod.ImplicitLinearAccelerationMethod:
                        return (double)1 / 6;

                    case NumericalMethod.NewmarkBeta:
                    case NumericalMethod.Newmark:
                        return 0.25;

                    case NumericalMethod.CentralDifferenceMethod:
                    case NumericalMethod.RungeKuttaForthOrder:
                        return 0;

                    default:
                        throw new NotImplementedException($"The numerical method '{this.NumericalMethod}' was not implemented.");
                }
            }
        }

        /// <summary>
        /// Integration constant used in numerical method calculations.
        /// For Gama = 1 / 2 the numerical method is at least second-order accurate, it is first order accurate for all other values of.
        /// </summary>
        public double Gama
        {
            get
            {
                switch (this.NumericalMethod)
                {
                    case NumericalMethod.CentralDifferenceMethod:
                    case NumericalMethod.ImplicitLinearAccelerationMethod:
                    case NumericalMethod.NewmarkBeta:
                    case NumericalMethod.Newmark:
                        return 0.5;

                    case NumericalMethod.RungeKuttaForthOrder:
                        return 0;

                    default:
                        throw new NotImplementedException($"The numerical method '{this.NumericalMethod}' was not implemented.");
                }
            }
        }
    }
}
