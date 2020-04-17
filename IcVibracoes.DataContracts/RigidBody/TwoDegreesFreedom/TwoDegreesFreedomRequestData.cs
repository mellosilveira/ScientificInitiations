using IcVibracoes.Common.Classes;

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
        public MechanicalProperties MainObjectMechanicalProperties { get; set; }

        /// <summary>
        /// The mechanical properties of the secondary object that will be analyzed.
        /// </summary>
        public MechanicalProperties SecondaryObjectMechanicalProperties { get; set; }
    }
}