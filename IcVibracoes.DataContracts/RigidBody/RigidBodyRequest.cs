using IcVibracoes.DataContracts.RigidBody;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.RigidBody
{
    /// <summary>
    /// It contains the request content to Rigid Body operation.
    /// </summary>
    public abstract class RigidBodyRequest<TRequestData> : OperationRequestBase
        where TRequestData : RigidBodyRequestData
    {
        /// <summary>
        /// It represents the 'data' content of Rigid Body request operation.
        /// </summary>
        [Required]
        public TRequestData Data { get; set; }
    }
}
