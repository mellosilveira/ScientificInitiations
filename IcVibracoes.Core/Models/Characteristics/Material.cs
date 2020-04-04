using System;
using static IcVibracoes.Common.Enum;

namespace IcVibracoes.Core.Models.Characteristics
{
    /// <summary>
    /// It contains the necessary informations about whatever material.
    /// </summary>
    public abstract class Material
    {
        /// <summary>
        /// Young modulus. Unity: Pa (Pascal).
        /// </summary>
        public abstract double YoungModulus { get; }

        /// <summary>
        /// Yield strength. Unity: Pa (Pascal).
        /// </summary>
        //public abstract double YieldStrength { get; }

        /// <summary>
        /// Specific mass. Unity: kg/m³ (kilogram per cubic meters).
        /// </summary>
        public abstract double SpecificMass { get; }
    }

    /// <summary>
    /// It contains the necessary informations about Steel SAE 1020.
    /// </summary>
    public class Steel1020 : Material
    {
        public override double YoungModulus => 205e9;

        //public override double YieldStrenght => 350e6;

        public override double SpecificMass => 7850;
    }

    /// <summary>
    /// It contains the necessary informations about Steel SAE 4130.
    /// </summary>
    public class Steel4130 : Material
    {
        public override double YoungModulus => 200e9;

        //public override double YieldStrenght => 460e6;

        public override double SpecificMass => 7850;
    }

    /// <summary>
    /// It contains the necessary informations about a common Aluminum.
    /// </summary>
    public class Aluminum : Material
    {
        public override double YoungModulus => 70e9;

        //public override double YieldStrenght => 300e6;

        public override double SpecificMass => 2710;
    }

    /// <summary>
    /// It's responsible to manipulate a material object based in a string.
    /// </summary>
    public class MaterialFactory
    {
        /// <summary>
        /// It's responsible to create a material object based in a string.
        /// </summary>
        public static Material Create(string material)
        {
            switch ((Materials)Enum.Parse(typeof(Materials), material.Trim(), ignoreCase: true))
            {
                case Materials.Steel1020: return new Steel1020();
                case Materials.Steel4130: return new Steel4130();
                case Materials.Aluminum: return new Aluminum();
                default: break;
            }

            throw new Exception($"Invalid material: {material}.");
        }

        /// <summary>
        /// It's responsible to validate a material object based in a string.
        /// </summary>
        public bool Validate(string material)
        {
            bool isMaterialValid = Enum.TryParse<Materials>(material, out _);

            return isMaterialValid;
        }
    }
}
