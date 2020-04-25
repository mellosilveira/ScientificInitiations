﻿namespace IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom
{
    /// <summary>
    /// It contains the request content of Rigid Body analysis with One Degree Freedom.
    /// </summary>
    public class OneDegreeFreedomRequest : RigidBodyRequest<OneDegreeFreedomRequestData> 
    {
        public override string AnalysisType 
        {
            get
            {
                return "RigidBody_OneDegreeFreedom";
            }
        }
    }
}