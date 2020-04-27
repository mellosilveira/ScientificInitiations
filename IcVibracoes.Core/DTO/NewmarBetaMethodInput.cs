using System.Collections.Generic;

namespace IcVibracoes.Core.DTO
{
    /// <summary>
    /// It contains the input 'data' to Newmark-Beta numerical method.
    /// </summary>
    public class NewmarBetaMethodInput
    {
        public double[,] Mass { get; set; }

        public double[,] Stiffness { get; set; }

        public double[,] Damping { get; set; }

        public double[] Force { get; set; }

        public List<double> AngularFrequencies { get; set; }

        public uint DegreesOfFreedom { get; set; }

        public double InitialTime
        {
            get => 0;
        }

        public double TimeStep { get; set; }

        public double FinalTime { get; set; }

        /// <summary>
        /// Integration constant used in Newmark-Beta method calculations.
        /// Valid values:
        /// 1. 0 --> Newmark-Beta method is identical to the central difference method.
        /// 2. 1/4 --> Newmark-Beta method is implicit and unconditionally stable. In this case the acceleration within the time interval [ti, ti+1) is presumed to be constant.
        /// 3. 1/6 --> Newmark-Beta method is identical to the linear acceleration method.
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// Integration constant used in Newmark-Beta method calculations.
        /// The Newmark-Beta method is conditionally stable if Gama < 1/2.
        /// For Gama = 1 / 2 the Newmark-Beta method is at least second-order accurate, it is firstorder accurate for all other values of.
        /// </summary>
        public double Gama
        {
            get => 1 / 2;
        }
    }
}
