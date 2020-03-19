using System;
using System.Collections.Generic;
using System.Text;
using static IcVibracoes.Common.Enum;

namespace IcVibracoes.Models.Beam.Characteristics
{
    public abstract class Fastening
    {
        public abstract bool Displacement { get; }

        public abstract bool Angle { get; }

    }

    public class Fixed : Fastening
    {
        public override bool Displacement => false;

        public override bool Angle => false;

    }

    public class Pinned : Fastening
    {
        public override bool Displacement => false;

        public override bool Angle => true;

    }

    public class None : Fastening
    {
        public override bool Displacement => true;

        public override bool Angle => true;

    }

    public class FasteningFactory
    {
        public static Fastening Create(string fastening)
        {
            switch ((Fastenings)Enum.Parse(typeof(Fastenings), fastening, ignoreCase: true))
            {
                case Fastenings.Fixed: return new Fixed();
                case Fastenings.Pinned: return new Pinned();
                case Fastenings.None: return new None();
            }

            throw new Exception();
        }
    }
}
