using System;

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
        /// <example>1e-4</example>
        public double? Area { get; set; }

        /// <summary>
        /// Profile moment of inertia.
        /// </summary>
        /// <example>8.33e-10</example>
        public double? MomentOfInertia { get; set; }
    }
}
