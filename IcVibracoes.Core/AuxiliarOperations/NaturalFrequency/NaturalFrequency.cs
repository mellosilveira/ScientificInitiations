using IcVibracoes.DataContracts;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.NaturalFrequency
{
    public class NaturalFrequency : INaturalFrequency
    {
        public Task<double[]> Calculate<TResponse, TResponseData>(TResponse response, double[,] mass, double[,] stiffness)
            where TResponseData : OperationResponseData
            where TResponse : OperationResponseBase<TResponseData>
        {
            int size = mass.GetLength(0);

            double[] naturalAngularFrequency = new double[size];

            return Task.FromResult(naturalAngularFrequency);
        }

        public Task<double> Calculate(double mass, double stiffness)
        {
            double naturalAngularFrequency = Math.Sqrt(mass / stiffness);

            return Task.FromResult(naturalAngularFrequency);
        }


    }
}
