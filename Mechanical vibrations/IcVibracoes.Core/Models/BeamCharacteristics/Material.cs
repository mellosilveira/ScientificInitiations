using System;

namespace IcVibracoes.Core.Models.BeamCharacteristics
{
    /// <summary>
    /// It contains the materials that can be used in the analysis.
    /// </summary>
    public enum MaterialType
    {
        Steel1020 = 1,
        Steel4130 = 2,
        Aluminum = 3
    }

    /// <summary>
    /// It contains the necessary information about whatever material.
    /// </summary>
    public struct Material
    {
        /// <summary>
        /// It contains the necessary information about Steel SAE 1020.
        /// YieldStrength => 350e6;
        /// </summary>
        public static readonly Material Steel1020 = new Material(youngModulus: 205e9, specificMass: 7850, MaterialType.Steel1020);

        /// <summary>
        /// It contains the necessary information about Steel SAE 4130.
        /// YieldStrength => 460e6
        /// </summary>
        public static readonly Material Steel4130 = new Material(youngModulus: 200e9, specificMass: 7850, MaterialType.Steel4130);

        /// <summary>
        /// It contains the necessary information about Aluminum.
        /// YieldStrength => 300e6
        /// </summary>
        public static readonly Material Aluminum = new Material(youngModulus: 70e9, specificMass: 2710, MaterialType.Aluminum);

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="youngModulus"></param>
        /// <param name="specificMass"></param>
        private Material(double youngModulus, double specificMass, MaterialType materialType)
        {
            YoungModulus = youngModulus;
            SpecificMass = specificMass;
            MaterialType = materialType;
        }

        /// <summary>
        /// Young modulus. Unity: Pa (Pascal).
        /// </summary>
        public double YoungModulus { get; }

        // <summary>
        // Yield strength. Unity: Pa (Pascal).
        // </summary>
        //public abstract double YieldStrength { get; }

        /// <summary>
        /// Specific mass. Unity: kg/m³ (kilogram per cubic meters).
        /// </summary>
        public double SpecificMass { get; }

        /// <summary>
        /// Material type
        /// </summary>
        public MaterialType MaterialType { get; }

        /// <summary>
        /// This method creates an instance of class <seealso cref="Material"/>.
        /// It can be <seealso cref="Steel1020"/>, <seealso cref="Steel4130"/> or <seealso cref="Aluminum"/>.
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static Material Create(string material)
        {
            return (MaterialType) Enum.Parse(typeof(MaterialType), material.Trim(), ignoreCase: true) switch
            {
                MaterialType.Steel1020 => Material.Steel1020,
                MaterialType.Steel4130 => Material.Steel4130,
                MaterialType.Aluminum => Material.Aluminum,
                
                _ => throw new Exception($"Invalid material: '{material}'.")
            };
        }
    }
}
