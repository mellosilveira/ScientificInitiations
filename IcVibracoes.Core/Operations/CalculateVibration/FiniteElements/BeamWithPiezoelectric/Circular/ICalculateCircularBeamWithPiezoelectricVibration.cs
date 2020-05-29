using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Circular
{
    /// <summary>
    /// It's responsible to calculate the vibration in a circular beam with piezoelectric.
    /// </summary>
    public interface ICalculateCircularBeamWithPiezoelectricVibration : ICalculateBeamWithPiezoelectricVibration<CircularProfile, NewmarkMethodInput>
    {
    }
}
