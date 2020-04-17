using IcVibracoes.Common.Profiles;

namespace IcVibracoes.DataContracts.FiniteElements.Beam
{
    /// <summary>
    /// It represents the 'data' content of beam request operation.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public class BeamRequestData<TProfile> : FiniteElementsRequestData<TProfile>
        where TProfile : Profile
    { }
}