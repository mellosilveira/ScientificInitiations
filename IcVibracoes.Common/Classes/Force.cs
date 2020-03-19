namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of the applied force .
    /// </summary>
    public class Force
    {
        /// <summary>
        /// Force node position.
        /// </summary>
        public uint NodePosition { get; set; }

        /// <summary>
        /// Force value.
        /// </summary>
        public double Value { get; set; }
    }
}
