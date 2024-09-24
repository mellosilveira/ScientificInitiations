using System.Linq;

namespace IcVibracoes.Core.ExtensionMethods
{
    /// <summary>
    /// It is responsible to create arrays.
    /// </summary>
    public class ArrayFactory
    {
        /// <summary>
        /// This method creates a vector with an unique value in the informed element positions with a size that is informed too.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="elementPositions"></param>
        /// <returns>A new instance of <see cref="double[]"/> with an unique value at the positions informed.</returns>
        public static double[] CreateVector(double value, uint size, uint[] elementPositions)
        {
            var newVector = new double[size];

            foreach (var t in elementPositions)
            {
                newVector[t - 1] = value;
            }

            return newVector;
        }

        /// <summary>
        /// This method creates a matrix with a unique value in all positions with a size that is informed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns>A new instance of <see cref="double[]"/> with an unique value at all positions.</returns>
        public static double[] CreateVector(double value, uint size) => Enumerable.Repeat(value, (int)size).ToArray();
    }
}