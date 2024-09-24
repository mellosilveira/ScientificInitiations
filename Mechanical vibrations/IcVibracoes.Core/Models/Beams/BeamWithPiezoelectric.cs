using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Models.BeamCharacteristics;

namespace IcVibracoes.Core.Models.Beams
{
    /// <summary>
    /// It represents a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamWithPiezoelectric<TProfile> : Beam<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Piezoelectric degrees of freedom.
        /// </summary>
        public uint PiezoelectricDegreesOfFreedom { get; set; }

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
        /// Electrical charge on piezoelectric surface.
        /// </summary>
        public double[] ElectricalCharge { get; set; }

        /// <summary>
        /// Piezoelectric specific mass.
        /// </summary>
        public double PiezoelectricSpecificMass { get; set; }

        /// <summary>
        /// Elements with piezoelectric.
        /// </summary>
        public uint[] ElementsWithPiezoelectric { get; set; }

        /// <summary>
        /// Number of piezoelectrics per elements. It can be positionated at top or bottom of the beam, top and bottom or around the bar.
        /// If is positionated at the top or at the bottom, number of piezoelectrics per elements = 1.
        /// If is positionated at the top and at the bottom, number of piezoelectrics per elements = 2.
        /// </summary>
        public uint NumberOfPiezoelectricPerElements { get; set; }

        /// <summary>
        /// Piezoelectric geometric properties.
        /// </summary>
        public GeometricProperty PiezoelectricGeometricProperty { get; set; }

        /// <summary>
        /// Piezoelectric profile.
        /// </summary>
        public TProfile PiezoelectricProfile { get; set; }
    }
}
