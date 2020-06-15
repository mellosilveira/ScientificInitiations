namespace IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' to Newmark and Newmark-Beta numerical method.
    /// </summary>
    public class NewmarkMethodInput : FiniteElementsMethodInput
    {
        /// <summary>
        /// Integration constant used in Newmark and Newmark-Beta method calculations.
        /// Valid values:
        /// 1. 0 --> Newmark method is similar to the central difference method.
        /// 2. 1/4 --> Newmark method is implicit and unconditionally stable. In this case the acceleration within the time interval [ti, ti+1) is presumed to be constant.
        /// 3. 1/6 --> Newmark method is similar to the linear acceleration method.
        /// </summary>
        public override double Beta => 0.25;

        /// <summary>
        /// Integration constant used in Newmark and Newmark-Beta method calculations.
        /// The Newmark method is conditionally stable if Gama < 1/2.
        /// For Gama = 1 / 2 the Newmark method is at least second-order accurate, it is first order accurate for all other values of.
        /// </summary>
        public override double Gama => 0.5;
    }
}
