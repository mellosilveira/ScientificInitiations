using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam.
    /// </summary>
    public interface ICalculateCircularBeamVibration : ICalculateBeamVibration<CircularProfile, NewmarkMethodInput>
    {
    }
}
