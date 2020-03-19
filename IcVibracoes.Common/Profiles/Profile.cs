namespace IcVibracoes.Common.Profiles
{
    /// <summary>
    /// It represents content in common between the profiles.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Profile area.
        /// </summary>
        public double? Area { get; set; }

        /// <summary>
        /// Profile moment of inertia.
        /// </summary>
        public double? MomentOfInertia { get; set; }
    }
}
