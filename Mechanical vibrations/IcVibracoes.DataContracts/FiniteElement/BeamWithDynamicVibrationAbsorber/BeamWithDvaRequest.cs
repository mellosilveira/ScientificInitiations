using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.FiniteElement.BeamWithDynamicVibrationAbsorber
{
    /// <summary>
    /// It represents the request content of CalculateBeamWithDva operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public sealed class BeamWithDvaRequest<TProfile> : FiniteElementRequest<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType => "FiniteElement_BeamWithDva";

        /// <summary>
        /// List of dynamic vibration absorber.
        /// </summary>
        [Required]
        public List<DynamicVibrationAbsorber> Dvas { get; set; }
    }
}
