using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric;

namespace IcVibracoes.Test.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric
{
    public class CalculateBeamWithPiezoelectricVibrationTest<TProfile>
        where TProfile : Profile, new()
    {
        protected CalculateBeamWithPiezoelectricVibration<TProfile> _operation;

        public CalculateBeamWithPiezoelectricVibrationTest()
        {

        }
    }
}
