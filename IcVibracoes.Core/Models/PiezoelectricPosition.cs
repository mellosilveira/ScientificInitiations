using System;
using System.Text.RegularExpressions;

namespace IcVibracoes.Core.Models
{
    public enum PiezoelectricPosition
    {
        Up = 1,
        Down = -1,
        UpAndDown = 2,
    }

    /// <summary>
    /// It's responsible to manipulate a PiezoelectricPosition object based in a string.
    /// </summary>
    public class PiezoelectricPositionFactory
    {
        /// <summary>
        /// It's responsible to create a PiezoelectricPosition object based in a string.
        /// </summary>
        public static uint Create(string piezoelectricPosition)
        {
            var value = Regex.Replace(piezoelectricPosition, @"\s", "");

            switch ((PiezoelectricPosition)Enum.Parse(typeof(PiezoelectricPosition), value, ignoreCase: true))
            {
                case PiezoelectricPosition.Up: return 1;
                case PiezoelectricPosition.Down: return 1;
                case PiezoelectricPosition.UpAndDown: return 2;
                default: break;
            }

            throw new Exception($"Invalid piezoelectric position: {piezoelectricPosition}.");
        }
    }
}
