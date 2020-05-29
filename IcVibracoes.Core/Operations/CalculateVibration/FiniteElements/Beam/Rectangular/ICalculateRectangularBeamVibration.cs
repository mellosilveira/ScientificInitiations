using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular beam.
    /// </summary>
    public interface ICalculateRectangularBeamVibration : ICalculateBeamVibration<RectangularProfile, NewmarkMethodInput>
    {
    }
}
