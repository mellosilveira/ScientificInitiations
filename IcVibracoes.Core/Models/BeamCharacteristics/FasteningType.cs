using System;

namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It represents the degrees of freedom for a generic fastening.
    /// </summary>
    public class FasteningType
    {
        public bool AllowLinearDisplacement { get; }

        public bool AllowAngularDisplacement { get; }

        private FasteningType(bool allowLinearDisplacement, bool allowAngularDisplacement)
        {
            AllowLinearDisplacement = allowLinearDisplacement;
            AllowAngularDisplacement = allowAngularDisplacement;
        }

        /// <summary>
        /// It represents the degrees of freedom for a fixed type fastening.
        /// </summary>
        public static readonly FasteningType Fixed = new FasteningType(false, false);

        /// <summary>
        /// It represents the degrees of freedom for a pinned type fastening.
        /// </summary>
        public static readonly FasteningType Pinned = new FasteningType(false, true);

        /// <summary>
        /// It represents the degrees of freedom for a case without fastening.
        /// </summary>
        public static readonly FasteningType None = new FasteningType(true, true);

        /// <summary>
        /// This method creates an instance of class <seealso cref="FasteningType"/>.
        /// It can be <seealso cref="None"/>, <seealso cref="Fixed"/> or <seealso cref="Pinned"/>.
        /// </summary>
        /// <param name="fastening"></param>
        /// <returns></returns>
        public static FasteningType Create(string fastening)
        {
            return (Fastenings) Enum.Parse(typeof(Fastenings), fastening, ignoreCase: true) switch
            {
                Fastenings.Fixed => Fixed,
                Fastenings.Pinned => Pinned,
                Fastenings.None => None,
                _ => throw new Exception($"Invalid fastening: '{fastening}'.")
            };
        }
    }

    /// <summary>
    /// The fastenings that can be used in the analysis.
    /// </summary>
    public enum Fastenings
    {
        None = 0,
        Pinned = 1,
        Fixed = 2,
    }
}
