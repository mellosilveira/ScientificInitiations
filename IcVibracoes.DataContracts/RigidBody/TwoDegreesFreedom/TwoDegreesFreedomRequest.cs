namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with Two Degree Freedom.
    /// </summary>
    public class TwoDegreesFreedomRequest : RigidBodyRequest<TwoDegreesFreedomRequestData> 
    {
        public override string AnalysisType 
        {
            get
            {
                return "RigidBody_TwoDegreesFreedom";
            }
        }
    }
}
