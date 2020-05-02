namespace IcVibracoes.Core.DTO.InputData.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' to Newmark numerical method.
    /// </summary>
    public class NewmarkMethodInput : FiniteElementsMethodInput
    {
        /// <summary>
        /// Integration constant used in Newmark-Beta method calculations.
        /// Valid values:
        /// 1. 0 --> Newmark-Beta method is identical to the central difference method.
        /// 2. 1/4 --> Newmark-Beta method is implicit and unconditionally stable. In this case the acceleration within the time interval [ti, ti+1) is presumed to be constant.
        /// 3. 1/6 --> Newmark-Beta method is identical to the linear acceleration method.
        /// </summary>
        public double Beta => 0.25;

        /// <summary>
        /// Integration constant used in Newmark-Beta method calculations.
        /// The Newmark-Beta method is conditionally stable if Gama < 1/2.
        /// For Gama = 1 / 2 the Newmark-Beta method is at least second-order accurate, it is firstorder accurate for all other values of.
        /// </summary>
        public double Gama => 0.5;
    }
}
