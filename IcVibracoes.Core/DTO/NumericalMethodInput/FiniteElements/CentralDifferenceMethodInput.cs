namespace IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' to Central Difference numerical method.
    /// </summary>
    public class CentralDifferenceMethodInput : FiniteElementsMethodInput
    {
        /// <summary>
        /// Integration constant used in Cental Difference method calculations.
        /// </summary>
        public double Beta => 0;

        /// <summary>
        /// Integration constant used in Cental Difference method calculations.
        /// </summary>
        public double Gama => 0.5;
    }
}
