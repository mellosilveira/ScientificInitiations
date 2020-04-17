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
        /// The hardness of the analyzed object.
        /// </summary>
        public double Hardness { get; set; }
    }
}
