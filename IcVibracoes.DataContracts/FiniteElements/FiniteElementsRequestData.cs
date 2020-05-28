using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.FiniteElements
{
    /// <summary>
    /// It represents the 'data' content of Finite Elements request operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class FiniteElementsRequestData<TProfile> : OperationRequestData
        where TProfile : Profile
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