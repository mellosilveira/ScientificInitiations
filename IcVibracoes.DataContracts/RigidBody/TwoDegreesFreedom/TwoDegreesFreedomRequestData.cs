using IcVibracoes.Common.Classes;
using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom
{
    /// <summary>
    /// It contains the request 'data' content of Rigid Body analysis with Two Degree Freedom.
    /// </summary>
    public class TwoDegreesFreedomRequestData : RigidBodyRequestData
    {
        /// <summary>
        /// The mechanical properties of the main object that will be analyzed.
        /// </summary>
        [Required]
        public MechanicalProperties MainObjectMechanicalProperties { get; set; }

        /// <summary>
        /// The mechanical properties of the secondary object that will be analyzed.
        /// </summary>
        [Required]
        public MechanicalProperties SecondaryObjectMechanicalProperties { get; set; }

        /// <summary>
        /// The initial displacement for primary object.
        /// Unit: m (meter)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double PrimaryInitialDisplacement { get; set; }

        /// <summary>
        /// The initial velocity for primary object.
        /// Unit: m/s (meters per second)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double PrimaryInitialVelocity { get; set; }

        /// <summary>
        /// The initial displacement for secondary object.,
        /// Unit: m (meter)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double SecondaryInitialDisplacement { get; set; }

        /// <summary>
        /// The initial velocity for secondary object.
        /// Unit: m/s (meters per second)
        /// </summary>
        /// <example>0</example>
        [Required]
        public double SecondaryInitialVelocity { get; set; }
    }
}