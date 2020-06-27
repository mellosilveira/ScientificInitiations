using IcVibracoes.DataContracts;
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
    /// It's responsible to manipulate a PiezoelectricPosition object based in a string.
    /// </summary>
    public class PiezoelectricPositionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="piezoelectricPosition"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static uint? Create<TResponseData>(string piezoelectricPosition, OperationResponseBase<TResponseData> response)
            where TResponseData : OperationResponseData
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

            response.AddError(OperationErrorCode.InternalServerError, $"Invalid piezoelectric position: {piezoelectricPosition}.");
            return null;
        }
    }
}
