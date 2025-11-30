using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva;

namespace IcVibracoes.Test.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva
{
    public class CalculateBeamWithDvaVibrationTest<TProfile>
        where TProfile : Profile, new()
    {
        protected CalculateBeamWithDvaVibration<TProfile> _operation;

        public CalculateBeamWithDvaVibrationTest()
        {

        }
    }
}
