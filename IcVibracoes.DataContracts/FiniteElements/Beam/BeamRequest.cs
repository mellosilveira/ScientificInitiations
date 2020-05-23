using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements.Beam
{
    /// <summary>
    /// It represents the request content of CalculateBeam operations.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamRequest<TProfile> : FiniteElementsRequest<TProfile, BeamRequestData<TProfile>>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// Tha analysis type.
        /// </summary>
        public override string AnalysisType 
        {
            get 
            {
                return "FiniteElements_Beam";
            }
        }
    }
}
