using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    public interface ICalculateRectangularBeamWithPiezoelectricVibration : ICalculateBeamWithPiezoelectricVibration<RectangularProfile, NewmarkMethodInput>
    {
    }
}
