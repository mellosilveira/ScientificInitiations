using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.FiniteElement.BeamWithPiezoelectric
{
    /// <summary>
    /// It represents the request content of CalculatePiezoelectric operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamWithPiezoelectricRequest<TProfile> : FiniteElementRequest<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType 
        {
            get
            {
                return "FiniteElement_BeamWithPiezoelectric";
            }
        }

        /// <summary>
        /// Piezoelectric Young Modulus.
        /// Unit: Pa (Pascal)
        /// </summary>
        /// <example>63e9</example>
        [Required]
        public double PiezoelectricYoungModulus { get; set; }

        /// <summary>
        /// Piezoelectric constant. Variable: d31.
        /// Unit: m/V (meter per Volts)
        /// </summary>
        /// <example>190e-12</example>
        [Required]
        public double PiezoelectricConstant { get; set; }

        /// <summary>
        /// Dielectric constant. Variable: k33.
        /// Unit: F/m (Farad per meter)
        /// </summary>
        /// <example>7.33e-9</example>
        [Required]
        public double DielectricConstant { get; set; }

        /// <summary>
        /// Elasticity value for constant electric field. Variable: c11.
        /// Unit: N/m² (Newton per square meter)
        /// </summary>
        /// <example>1.076e11</example>
        [Required]
        public double ElasticityConstant { get; set; }

        /// <summary>
        /// Dielectric permissiveness. Variable: e31.
        /// Unit: C/m² (Coulomb per square meter)
        /// </summary>
        /// <example>30.705</example>
        [Required]
        public double DielectricPermissiveness { get; set; }

        /// <summary>
        /// Electrical charges on piezoelectric surface.
        /// </summary>
        public List<ElectricalCharge> ElectricalCharges { get; set; }

        /// <summary>
        /// Piezoelectric specific mass.
        /// Unit: kg/m³ (kilogrameter per cubic meter)
        /// </summary>
        /// <example>7650</example>
        [Required]
        public double PiezoelectricSpecificMass { get; set; }

        /// <summary>
        /// Elements with piezoelectric.
        /// When it is not passed, is assumed that all elements have piezoelectric, because already exist an endpoint to do a anlysis in a beam without piezoelectric.
        /// </summary>
        /// <example>1, 2</example>
        public uint[] ElementsWithPiezoelectric { get; set; }

        /// <summary>
        /// Piezoelectric position on the beam. Can be: up, down, up and down.
        /// </summary>
        /// <example>Up And Down</example>
        [Required]
        public string PiezoelectricPosition { get; set; }

        /// <summary>
        /// Piezoelectric profile.
        /// </summary>
        /// <example>RectangularProfile</example>
        [Required]
        public TProfile PiezoelectricProfile { get; set; }
    }
}
