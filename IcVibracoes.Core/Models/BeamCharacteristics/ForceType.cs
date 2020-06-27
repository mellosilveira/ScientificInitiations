namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It contains the force types that is used to calculate the force.
    /// </summary>
    public enum ForceType
    {
        /// <summary>
        /// The force has sinusoidal behavior.
        /// </summary>
        Harmonic = 1,

        /// <summary>
        /// A force applied at an unique instante of time.
        /// </summary>
        Impact = 2
    }
}
