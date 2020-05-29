using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Rectangular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a rectangular beam with dynamic vibration absorber.
    /// </summary>
    public interface ICalculateRectangularBeamWithDvaVibration : ICalculateBeamWithDvaVibration<RectangularProfile, NewmarkMethodInput>
    {
    }
}
