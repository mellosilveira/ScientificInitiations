using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber
{
    /// <summary>
    /// It represents the request content of CalculateBeamWithDva operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamWithDvaRequest<TProfile> : FiniteElementsRequest<TProfile, BeamWithDvaRequestData<TProfile>>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType 
        { 
            get
            {
                return "FiniteElements_BeamWithDva";
            }
        }
    }
}
