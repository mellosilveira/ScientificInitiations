using FluentAssertions;

namespace IcVibracoes.Test.Core
{
    public static class HelperOperations
    {
        public static void ShouldBeBeApproximately(this double[] result, double[] area, double precision)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i].Should().BeApproximately(area[i], precision);
            }
        }

        public static void ShouldBeBeApproximately(this double[,] result, double[,] matrix, double precision)
        {
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result.Length; j++)
                {
                    result[i, j].Should().BeApproximately(matrix[i, j], precision);
                }
            }
        }
    }
}
