namespace IcVibracoes.Core.DTO.NumericalMethodInput
{
    /// <summary>
    /// It contains the input 'data' to all numerical methods.
    /// </summary>
    public class NumericalMethodInput
    {
        /// <summary>
        /// The initial time.
        /// Unit: s (second).
        /// </summary>
        public double InitialTime
        {
            get => 0;
        }

        /// <summary>
        /// The time step.
        /// Unit: s (second).
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// The final time.
        /// Unit: s (second).
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// The angular frequency.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double AngularFrequency { get; set; }

        /// <summary>
        /// The angular frequency step.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double AngularFrequencyStep { get; set; }

        /// <summary>
        /// The final angular frequency.
        /// Unit: Hz (Hertz).
        /// </summary>
        public double FinalAngularFrequency { get; set; }
    }
}
