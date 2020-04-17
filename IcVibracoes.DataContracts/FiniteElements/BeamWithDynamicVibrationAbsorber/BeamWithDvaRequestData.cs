using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElements.Beam;
using System.Collections.Generic;

namespace IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber
{
    /// <summary>
    /// It represents the 'data' content of BeamWithDva request.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamWithDvaRequestData<TProfile> : BeamRequestData<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// List of dynamic vibration absorber.
        /// </summary>
        public List<DynamicVibrationAbsorber> Dvas { get; set; }
    }
}
