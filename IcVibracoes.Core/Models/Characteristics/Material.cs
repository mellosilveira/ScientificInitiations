using System;
using System.Collections.Generic;
using System.Text;
using static IcVibracoes.Common.Enum;

namespace IcVibracoes.Models.Beam.Characteristics
{
    public abstract class Material
    {
        // Unity: Pa
        public abstract double YoungModulus { get; }

        // Unity: Pa
        //public abstract double YieldStrenght { get; }

        // Unity: kg/m³
        public abstract double SpecificMass { get; }
    }

    public class Steel1020 : Material
    {
        public override double YoungModulus => 205e9;

        //public override double YieldStrenght => 350e6;

        public override double SpecificMass => 7850;
    }

    public class Steel4130 : Material
    {
        public override double YoungModulus => 200e9;

        //public override double YieldStrenght => 460e6;

        public override double SpecificMass => 7850;
    }

    public class Aluminum : Material
    {
        public override double YoungModulus => 70e9;

        //public override double YieldStrenght => 300e6;

        public override double SpecificMass => 2710;
    }

    public class MaterialFactory
    {
        public static Material Create(string material)
        {
            switch ((Materials)Enum.Parse(typeof(Materials), material, ignoreCase: true))
            {
                case Materials.Steel1020: return new Steel1020();
                case Materials.Steel4130: return new Steel4130();
                case Materials.Aluminum: return new Aluminum();
            }

            throw new Exception();
        }
    }
}
