using IcVibracoes.Common.Classes;

namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with Two Degrees of Freedom.
    /// </summary>
    public class TwoDegreesOfFreedomRequest : RigidBodyRequest 
    {
        /// <summary>
        /// The analysis type. 
        /// </summary>
        public override string AnalysisType 
        {
            get
            {
                return "RigidBody_TwoDegreesFreedom";
            }
        }

        /// <summary>
        /// The data of main element to be analyzed.
        /// </summary>
        public MechanicalProperties PrimaryElementData { get; set; }

        /// <summary>
        /// The data of secondary element to be analyzed.
        /// </summary>
        public MechanicalProperties SecondaryElementData { get; set; }
    }
}
