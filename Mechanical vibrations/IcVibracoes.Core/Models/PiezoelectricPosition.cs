using System;
using System.Text.RegularExpressions;

namespace IcVibracoes.Core.Models
{
    /// <summary>
    /// The piezoelectric positions that can be used in the analysis.
    /// </summary>
    public enum PiezoelectricPosition
    {
        Up = 1,
        Down = -1,
        UpAndDown = 2,
    }

    /// <summary>
    /// It's responsible to manipulate the enum <see cref="PiezoelectricPosition"/> based in a string.
    /// </summary>
    public class PiezoelectricPositionFactory
    {
        /// <summary>
        /// This method returns the number of piezoelectrics per element.
        /// </summary>
        /// <param name="piezoelectricPosition"></param>
        /// <returns></returns>
        public static uint Create(string piezoelectricPosition)
        {
            var value = Regex.Replace(piezoelectricPosition, @"\s", "");

            switch ((PiezoelectricPosition)Enum.Parse(typeof(PiezoelectricPosition), value, ignoreCase: true))
            {
                case PiezoelectricPosition.Up:
                    return 1;
                case PiezoelectricPosition.Down:
                    return 1;
                case PiezoelectricPosition.UpAndDown:
                    return 2;
                default: break;
            }

            throw new Exception($"Invalid piezoelectric position: {piezoelectricPosition}.");
        }
    }
}
