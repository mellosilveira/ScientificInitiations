using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.FiniteElement
{
    /// <summary>
    /// It represents the request content of Finite Element operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class FiniteElementRequest<TProfile> : OperationRequestBase
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Number of elements in the beam.
        /// </summary>
        /// <example>2</example>
        [Required]
        public uint NumberOfElements { get; set; }

        /// <summary>
        /// Beam material.
        /// </summary>
        /// <example>Steel 1020</example>
        [Required]
        public string Material { get; set; }

        /// <summary>
        /// Beam length.
        /// Unit: m (meter)
        /// </summary>
        /// <example>0.5</example>
        [Required]
        public double Length { get; set; }

        /// <summary>
        /// The beam fastenings.
        /// </summary>
        [Required]
        public List<Fastening> Fastenings { get; set; }

        /// <summary>
        /// Applied forces in the beam.
        /// </summary>
        [Required]
        public List<Force> Forces { get; set; }

        /// <summary>
        /// Beam profile.
        /// </summary>
        /// <example>RectangularProfile</example>
        [Required]
        public TProfile Profile { get; set; }
    }
}
