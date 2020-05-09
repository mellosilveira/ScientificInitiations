using System;

namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It represents the degrees of freedom for a generic fastening.
    /// </summary>
    public abstract class FasteningType
    {
        public abstract bool LinearDisplacement { get; }

        public abstract bool AngularDisplacement { get; }

    }

    /// <summary>
    /// It represents the degrees of freedom for a fixed type fastening.
    /// </summary>
    public class Fixed : FasteningType
    {
        public override bool LinearDisplacement => false;

        public override bool AngularDisplacement => false;

    }

    /// <summary>
    /// It represents the degrees of freedom for a pinned type fastening.
    /// </summary>
    public class Pinned : FasteningType
    {
        public override bool LinearDisplacement => false;

        public override bool AngularDisplacement => true;

    }

    /// <summary>
    /// It represents the degrees of freedom for a case without fastening.
    /// </summary>
    public class None : FasteningType
    {
        public override bool LinearDisplacement => true;

        public override bool AngularDisplacement => true;

    }

    /// <summary>
    /// It's responsible to create a fastening object based on a string.
    /// </summary>
    public class FasteningFactory
    {
        public static FasteningType Create(string fastening)
        {
            switch ((Fastenings)Enum.Parse(typeof(Fastenings), fastening, ignoreCase: true))
            {
                case Fastenings.Fixed: return new Fixed();
                case Fastenings.Pinned: return new Pinned();
                case Fastenings.None: return new None();
                default: break;
            }

            throw new Exception($"Invalid fastening: {fastening}.");
        }
    }

    public enum Fastenings
    {
        None,
        Pinned,
        Fixed,
    }
}
