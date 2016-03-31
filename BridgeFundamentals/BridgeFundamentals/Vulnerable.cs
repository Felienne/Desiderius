using System;

namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Summary description for Vulnerable.
    /// </summary>
    public enum Vulnerable { Neither, NS, EW, Both }

    public static class VulnerableConverter
    {
        public static Vulnerable FromXML(string value)
        {
            switch (value.Trim().ToLower())
            {
                case "-":
                case "none":
                case "neither":
                    return Vulnerable.Neither;
                case "ns":
                    return Vulnerable.NS;
                case "ew":
                    return Vulnerable.EW;
                case "both":
                case "all":
                    return Vulnerable.Both;
                default:
                    throw new ArgumentOutOfRangeException(value);
            }
        }
        public static Vulnerable FromBoardNumber(int value)
        {
            int board = 1 + ((((int)value) - 1) % 16);
            switch (board)
            {
                case 1:
                case 8:
                case 11:
                case 14: return Vulnerable.Neither;
                case 2:
                case 5:
                case 12:
                case 15: return Vulnerable.NS;
                case 3:
                case 6:
                case 9:
                case 16: return Vulnerable.EW;
                default: return Vulnerable.Both;
            }
        }

        public static string ToXML(Vulnerable value)
        {
            switch ((Vulnerable)value)
            {
                case Vulnerable.Neither: return "None";
                case Vulnerable.NS: return "NS";
                case Vulnerable.EW: return "EW";
                default: return "All";
            }
        }

        //public static string ToString(Vulnerable value)
        //{
        //    switch ((Vulnerable)value)
        //    {
        //        case Vulnerable.Neither: return LocalizationResources.Neither;
        //        case Vulnerable.NS: return LocalizationResources.NS;
        //        case Vulnerable.EW: return LocalizationResources.EW;
        //        default: return LocalizationResources.All;
        //    }
        //}

        /// <summary>
        /// Localized string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Localized string</returns>
        public static string ToString2(this Vulnerable value)
        {
            switch ((Vulnerable)value)
            {
                case Vulnerable.Neither: return LocalizationResources.Neither;
                case Vulnerable.NS: return LocalizationResources.NS;
                case Vulnerable.EW: return LocalizationResources.EW;
                default: return LocalizationResources.All;
            }
        }

        public static string ToBridgeProtocol(Vulnerable value)
        {
            switch ((Vulnerable)value)
            {
                case Vulnerable.Neither: return "Neither";
                case Vulnerable.NS: return "N/S";
                case Vulnerable.EW: return "E/W";
                default: return "Both";
            }
        }

        public static Vulnerable Rotate(Vulnerable value)
        {
            switch ((Vulnerable)value)
            {
                case Vulnerable.Neither: return Vulnerable.Neither;
                case Vulnerable.NS: return Vulnerable.EW;
                case Vulnerable.EW: return Vulnerable.NS;
                default: return Vulnerable.Both;
            }
        }
    }
}
