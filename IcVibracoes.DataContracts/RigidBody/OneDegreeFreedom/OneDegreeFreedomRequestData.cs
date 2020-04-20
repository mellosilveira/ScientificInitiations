using IcVibracoes.Common.Classes;

namespace IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom
{
    /// <summary>
    /// It contains the request 'data' content of Rigid Body analysis with One Degree Freedom.
    /// </summary>
    public class OneDegreeFreedomRequestData : RigidBodyRequestData
    {
        /// <summary>
        /// The mechanical properties of the object that will be analyzed.
        /// </summary>
        public MechanicalProperties MechanicalProperties { get; set; }

        /// <summary>
        /// The initial displacement.
        /// </summary>
        public double InitialDisplacement { get; set; }

        /// <summary>
        /// The initial velocity.
        /// </summary>
        public double InitialVelocity { get; set; }
    }
}