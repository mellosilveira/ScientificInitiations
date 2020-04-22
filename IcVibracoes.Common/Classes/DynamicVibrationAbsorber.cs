namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It represents the content of DynamicVibrationAbsorber.
    /// </summary>
    public class DynamicVibrationAbsorber
    {
        /// <summary>
        /// Mass of DVA.
        /// </summary>
        public double DvaMass { get; set; }

        /// <summary>
        /// Stiffness of DVA.
        /// </summary>
        public double DvaStiffness { get; set; }

        /// <summary>
        /// Node position of DVA.
        /// </summary>
        public uint DvaNodePosition { get; set; }
    }
}
