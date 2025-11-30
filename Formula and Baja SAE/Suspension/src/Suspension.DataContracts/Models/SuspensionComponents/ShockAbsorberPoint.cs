namespace MudRunner.Suspension.DataContracts.Models.SuspensionComponents
{
    /// <summary>
    /// It represents the points to shock absorber.
    /// </summary>
    public class ShockAbsorberPoint : SingleComponentPoint
    {
        /// <summary>
        /// Creates a <see cref="ShockAbsorberPoint"/> based on a <see cref="ShockAbsorber"/>.
        /// </summary>
        /// <param name="shockAbsorber"></param>
        /// <returns></returns>
        public static ShockAbsorberPoint Create(ShockAbsorber shockAbsorber)
        {
            return new ShockAbsorberPoint
            {
                FasteningPoint = shockAbsorber.FasteningPoint,
                PivotPoint = shockAbsorber.PivotPoint
            };
        }
    }
}
