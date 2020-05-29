namespace IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with One Degree of Freedom.
    /// </summary>
    public class OneDegreeOfFreedomRequest : RigidBodyRequest<OneDegreeOfFreedomRequestData> 
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
    }
}
