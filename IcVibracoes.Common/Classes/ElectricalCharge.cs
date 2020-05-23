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
        /// <example></example>
        public double Value { get; set; }

        /// <summary>
        /// Electrical charge node position.
        /// </summary>
        /// <example>0</example>
        public uint NodePosition { get; set; }
    }
}
