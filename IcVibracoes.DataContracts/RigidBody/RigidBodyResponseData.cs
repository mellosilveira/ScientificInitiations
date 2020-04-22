using System.Collections.Generic;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the 'data' content of RigidBody response operation.
    /// </summary>
    public class RigidBodyResponseData : OperationResponseData 
    {
        /// <summary>
        /// The analysis result.
        /// The dictionary key is the time.
        /// </summary>
        public IDictionary<double, double[]> Results { get; set; }
    }
}