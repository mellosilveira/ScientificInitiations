using System.Collections.Generic;

namespace IcVibracoes.Core.DTO.InputData.FiniteElements
{
    /// <summary>
    /// It contains the input 'data' of finite element methods.
    /// </summary>
    public class FiniteElementsMethodInput
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
    }
}
