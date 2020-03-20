using System;
using System.Threading.Tasks;

namespace IcVibracoes.Calculator.GeometricProperties
{
    /// <summary>
    /// It's responsible to calculate any geometric property.
    /// </summary>
    public class CalculateGeometricProperty : ICalculateGeometricProperty
    {
        /// <summary>
        /// Method to calculate the area to circular profile.
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public Task<double> Area(double diameter, double? thickness)
        {
            double area;

            if (thickness == null)
            {
                area = Math.PI * Math.Pow(diameter, 2) / 4;
            }
            else
            {
                area = (Math.PI / 4) * (Math.Pow(diameter, 2) - Math.Pow(diameter - 2 * thickness.Value, 2));
            }

            return Task.FromResult(area);
        }

        /// <summary>
        /// Method to calculate the area to rectangular or square profile.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public Task<double> Area(double height, double width, double? thickness)
        {
            double area;

            if (thickness == null)
            {
                area = height * width;
            }
            else
            {
                area = (height * width) - ((height - 2 * thickness.Value) * (width - 2 * thickness.Value));
            }

            return Task.FromResult(area);
        }

        /// <summary>
        /// Method to calculate the moment of inertia to circular profile.
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public Task<double> MomentOfInertia(double diameter, double? thickness)
        {
            double momentOfInertia;

            if (thickness == null)
            {
                momentOfInertia = Math.PI * Math.Pow(diameter, 4) / 64;
            }
            else
            {
                momentOfInertia = (Math.PI / 64) * (Math.Pow(diameter, 4) - Math.Pow(diameter - 2 * thickness.Value, 4));
            }

            return Task.FromResult(momentOfInertia);
        }

        /// <summary>
        /// Method to calculate the moment of inertia to rectangular or square profile.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public Task<double> MomentOfInertia(double height, double width, double? thickness)
        {
            double momentOfInertia;

            if (thickness == null)
            {
                momentOfInertia = Math.Pow(height, 3) * width / 12;
            }
            else
            {
                momentOfInertia = (Math.Pow(height, 3) * width - (Math.Pow(height - 2 * thickness.Value, 3) * (width - 2 * thickness.Value))) / 12;
            }

            return Task.FromResult(momentOfInertia);
        }
    }
}
