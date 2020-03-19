using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.CalculateVibration.Beam;
using System.Collections.Generic;
using static IcVibracoes.Common.Enum;

namespace IcVibracoes.DataContracts.CalculateVibration.BeamWithPiezoelectric
{
    /// <summary>
    /// It represents the 'data' content of piezoelectric request.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class PiezoelectricRequestData<TProfile> : BeamRequestData<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// Piezoelectric Young Modulus.
        /// </summary>
        public double PiezoelectricYoungModulus { get; set; }

        /// <summary>
        /// Piezoelectric constant. Variable: d31.
        /// </summary>
        public double PiezoelectricConstant { get; set; }

        /// <summary>
        /// Dielectric constant. Variable: k33.
        /// </summary>
        public double DielectricConstant { get; set; }

        /// <summary>
        /// Dielectric permissiveness. Variable: e31.
        /// </summary>
        public double DielectricPermissiveness { get; set; }

        /// <summary>
        /// Elasticity value for constant electric field. Variable: c11.
        /// </summary>
        public double ElasticityConstant { get; set; }

        /// <summary>
        /// Electrical charges on piezoelectric surface.
        /// </summary>
        public List<ElectricalCharge> ElectricalCharges { get; set; }

        /// <summary>
        /// Piezoelectric specific mass.
        /// </summary>
        public double PiezoelectricSpecificMass { get; set; }

        /// <summary>
        /// Elements with piezoelectric.
        /// </summary>
        public uint[] ElementsWithPiezoelectric { get; set; }

        /// <summary>
        /// Piezoelectric position on the beam. Can be: up, down, up and down, around.
        /// </summary>
        public string PiezoelectricPosition { get; set; } 

        /// <summary>
        /// Piezoelectric profile.
        /// </summary>
        public TProfile PiezoelectricProfile { get; set; }
    }
}
