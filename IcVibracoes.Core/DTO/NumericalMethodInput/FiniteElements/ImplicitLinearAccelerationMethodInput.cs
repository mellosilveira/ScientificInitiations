namespace IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' to Implicit Linear Acceleration numerical method.
    /// </summary>
    public class ImplicitLinearAccelerationMethodInput : FiniteElementsMethodInput
    {
        /// <summary>
        /// Integration constant used in Implicit Linear Acceleration method calculations.
        /// </summary>
        public double Beta 
        { 
            get
            {
                double value = 1 / 6;
                return value;
            }
        }

        /// <summary>
        /// Integration constant used in Implicit Linear Acceleration method calculations.
        /// </summary>
        public double Gama => 0.5;
    }
}
