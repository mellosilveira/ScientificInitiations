namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with Two Degrees of Freedom.
    /// </summary>
    public class TwoDegreesOfFreedomRequest : RigidBodyRequest<TwoDegreesOfFreedomRequestData> 
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
    }
}
