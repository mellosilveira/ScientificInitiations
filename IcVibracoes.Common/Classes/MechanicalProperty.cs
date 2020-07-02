using System.ComponentModel.DataAnnotations;

namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It contains the mechanical properties to be used in Rigid Body analysis.
    /// </summary>
    public class MechanicalProperty
    {
        /// <summary>
        /// The mass of the analyzed object.
        /// Unit: kg (kilogram)
        /// </summary>
        /// <example>20</example>
        [Required]
        public double Mass { get; set; }

        /// <summary>
        /// The stiffness of the analyzed object.
        /// Unit: N/m (Newton per meter)
        /// </summary>
        /// <example>5000</example>
        [Required]
        public double Stiffness { get; set; }
    }
}
