using IcVibracoes.DataContracts;
using System.Threading.Tasks;

namespace IcVibracoes.Core.AuxiliarOperations.NaturalAngularFrequency
{
    public interface INaturalAngularFrequency
    {
        Task<double[]> Calculate<TResponse, TResponseData>(TResponse response, double[,] mass, double[,] stiffness)
            where TResponse : OperationResponseBase<TResponseData>
            where TResponseData : OperationResponseData;

        Task<double> Calculate(double mass, double stiffness);
    }
}
