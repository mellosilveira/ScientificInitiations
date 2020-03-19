namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of applied electric charge on piezoelectric surface.
    /// </summary>
    public class ElectricalCharge
    {
        /// <summary>
        /// Electrical charge value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Electrical charge node position.
        /// </summary>
        public uint NodePosition { get; set; }
    }
}
