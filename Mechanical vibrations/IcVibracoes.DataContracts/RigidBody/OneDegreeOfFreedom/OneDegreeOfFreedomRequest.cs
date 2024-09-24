using IcVibracoes.Common.Classes;

namespace IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with One Degree of Freedom.
    /// </summary>
    public sealed class OneDegreeOfFreedomRequest : RigidBodyRequest 
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType => "RigidBody_OneDegreeFreedom";

        /// <summary>
        /// The data of element to be analyzed.
        /// </summary>
        public MechanicalProperty ElementData { get; set; }
    }
}
