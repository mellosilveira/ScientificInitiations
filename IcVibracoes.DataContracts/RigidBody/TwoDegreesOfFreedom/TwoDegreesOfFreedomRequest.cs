using IcVibracoes.Common.Classes;

namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with Two Degrees of Freedom.
    /// </summary>
    public sealed class TwoDegreesOfFreedomRequest : RigidBodyRequest 
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType => "RigidBody_TwoDegreesOfFreedom";

        /// <summary>
        /// The data of main element to be analyzed.
        /// </summary>
        public MechanicalProperty PrimaryElementData { get; set; }

        /// <summary>
        /// The data of secondary element to be analyzed.
        /// </summary>
        public MechanicalProperty SecondaryElementData { get; set; }
    }
}
