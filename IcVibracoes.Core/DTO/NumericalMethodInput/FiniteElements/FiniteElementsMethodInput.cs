using IcVibracoes.Core.Models;
using System;

namespace IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement
{
    /// <summary>
    /// It contains the input 'data' of finite element methods.
    /// </summary>
    public class FiniteElementMethodInput : NumericalMethodInput
    {
        /// <summary>
        /// The numerical method used in the finite element analysis.
        /// </summary>
        public NumericalMethod NumericalMethod { get; set; }

        /// <summary>
        /// The mass matrix.
        /// </summary>
        public double[,] Mass { get; set; }

        /// <summary>
        /// The stiffness matrix.
        /// </summary>
        public double[,] Stiffness { get; set; }

        /// <summary>
        /// The damping matrix.
        /// </summary>
        public double[,] Damping { get; set; }

        /// <summary>
        /// The force matrix with original values.
        /// </summary>
        public double[] OriginalForce { get; set; }

        /// <summary>
        /// The force matrix with values for a specific time.
        /// </summary>
        public double[] Force { get; set; }

        /// <summary>
        /// The number of true boundary conditions.
        /// </summary>
        public uint NumberOfTrueBoundaryConditions { get; set; }

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
