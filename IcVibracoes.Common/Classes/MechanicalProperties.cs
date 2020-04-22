namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// It contains the mechanical properties to be used in Rigid Body analysis.
    /// </summary>
    public class MechanicalProperties
    {
        /// <summary>
        /// The mass of the analyzed object.
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The stiffness of the analyzed object.
        /// </summary>
        public double Stiffness { get; set; }
    }
}
