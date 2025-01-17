﻿using System;

namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It represents the degrees of freedom for a generic fastening.
    /// </summary>
    public abstract class FasteningType
    {
        public abstract bool AlowLinearDisplacement { get; }

        public abstract bool AlowAngularDisplacement { get; }
    }

    /// <summary>
    /// It represents the degrees of freedom for a fixed type fastening.
    /// </summary>
    public class Fixed : FasteningType
    {
        public override bool AlowLinearDisplacement => false;

        public override bool AlowAngularDisplacement => false;
    }

    /// <summary>
    /// It represents the degrees of freedom for a pinned type fastening.
    /// </summary>
    public class Pinned : FasteningType
    {
        public override bool AlowLinearDisplacement => false;

        public override bool AlowAngularDisplacement => true;
    }

    /// <summary>
    /// It represents the degrees of freedom for a case without fastening.
    /// </summary>
    public class None : FasteningType
    {
        public override bool AlowLinearDisplacement => true;

        public override bool AlowAngularDisplacement => true;
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

    /// <summary>
    /// It's responsible to manipulate the enum <see cref="Fastenings"/> based in a string.
    /// </summary>
    public class FasteningFactory
    {
        /// <summary>
        /// This method creates an instance of class <seealso cref="FasteningType"/>.
        /// It can be <seealso cref="None"/>, <seealso cref="Fixed"/> or <seealso cref="Pinned"/>.
        /// </summary>
        /// <param name="fastening"></param>
        /// <returns></returns>
        public static FasteningType Create(string fastening)
        {
            switch ((Fastenings)Enum.Parse(typeof(Fastenings), fastening, ignoreCase: true))
            {
                case Fastenings.Fixed:
                    return new Fixed();
                case Fastenings.Pinned:
                    return new Pinned();
                case Fastenings.None:
                    return new None();
                default:
                    break;
            }

            throw new Exception($"Invalid fastening: '{fastening}'.");
        }
    }
}
