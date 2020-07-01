using IcVibracoes.Common.Classes;

namespace IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with One Degree of Freedom.
    /// </summary>
    public class OneDegreeOfFreedomRequest : RigidBodyRequest 
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType 
        {
            get
            {
                return "RigidBody_OneDegreeFreedom";
            }
        }

        /// <summary>
        /// The data of element to be analyzed.
        /// </summary>
        public MechanicalProperties ElementData { get; set; }
    }
}
