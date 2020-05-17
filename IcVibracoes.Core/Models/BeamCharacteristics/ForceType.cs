using System;

namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It contains the force types that is used to calculate the force.
    /// </summary>
    public enum ForceType
    {
        Harmonic = 1,
        Impact = 2
    }

    /// <summary>
    /// It's responsible to create a force type object based on a string.
    /// </summary>
    public class ForceTypeFactory
    {
        public static ForceType Create(string forceType)
        {
            switch ((ForceType)Enum.Parse(typeof(ForceType), forceType, ignoreCase: true))
            {
                case ForceType.Harmonic: return ForceType.Harmonic;
                case ForceType.Impact: return ForceType.Impact;
                default: break;
            }

            throw new Exception($"Invalid force type: {forceType}.");
        }
    }
}
