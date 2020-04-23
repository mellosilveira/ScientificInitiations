namespace IcVibracoes.Common.ExtensionMethods
{
    public static class VectorExtension
    {
        public static double GetMaxValue(this double[] vector)
        {
            double maxValue = vector[0];
            for (int i = 1; i < vector.Length; i++)
            {
                if (vector[i] > maxValue)
                {
                    maxValue = vector[i];
                }
            }

            return maxValue;
        }

        public static double GetMinValue(this double[] vector)
        {
            double minValue = vector[0];
            for (int i = 1; i < vector.Length; i++)
            {
                if (vector[i] < minValue)
                {
                    minValue = vector[i];
                }
            }

            return minValue;
        }

        /// <summary>
        /// Get the first index of value.
        /// If index don't exist returns -1.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this double[] vector, double value)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public static double[] Divide(this double[] vector, double[] vectorToDivide)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / vectorToDivide[i];
            }

            return result;
        }

        public static double[] DivideEachElement(this double[] vector, double value)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / value;
            }

            return result;
        }
    }
}
