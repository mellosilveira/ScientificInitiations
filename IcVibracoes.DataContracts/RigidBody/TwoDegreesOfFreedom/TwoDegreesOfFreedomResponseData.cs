namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom
{
    /// <summary>
    /// It contains the response 'data' content of Rigid Body analysis with Two Degrees of Freedom.
    /// </summary>
    public class TwoDegreesOfFreedomResponseData : RigidBodyResponseData
    {
        /// <summary>
        /// The time corresponding for each value in that class.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Displacement of the primary object analyzed.
        /// </summary>
        public double PrimaryDisplacement { get; set; }

        /// <summary>
        /// Velocity of the primary object analyzed.
        /// </summary>
        public double PrimaryVelocity { get; set; }

        /// <summary>
        /// Displacement of the secondary object analyzed.
        /// </summary>
        public double SecondaryDisplacement { get; set; }

        /// <summary>
        /// Velocity of the secondary object analyzed.
        /// </summary>
        public double SecondaryVelocity { get; set; }
    }
}
