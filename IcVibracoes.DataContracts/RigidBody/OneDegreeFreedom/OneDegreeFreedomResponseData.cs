namespace IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom
{
    /// <summary>
    /// It contains the response 'data' content of Rigid Body analysis with One Degree Freedom.
    /// </summary>
    public class OneDegreeFreedomResponseData : RigidBodyResponseData
    {
        /// <summary>
        /// The time corresponding for each value in that class.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Displacement of the object analyzed.
        /// </summary>
        public double Displacement { get; set; }

        /// <summary>
        /// Velocity of the object analyzed.
        /// </summary>
        public double Velocity { get; set; }
    }
}
