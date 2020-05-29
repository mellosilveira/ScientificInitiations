using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with dynamic vibration absorber.
    /// </summary>
    public interface ICalculateCircularBeamWithDvaVibration : ICalculateBeamWithDvaVibration<CircularProfile, NewmarkMethodInput>
    {
    }
}
