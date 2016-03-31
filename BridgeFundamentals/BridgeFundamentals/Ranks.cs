
namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Summary description for CardPlay.
    /// </summary>

    public enum VirtualRanks { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public enum Ranks { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public static class Rank
    {
        public const int Ace = (int)Ranks.Ace;
        public const int King = (int)Ranks.King;
        public const int Queen = (int)Ranks.Queen;
        public const int Jack = (int)Ranks.Jack;
        public const int Ten = (int)Ranks.Ten;
        public const int Nine = (int)Ranks.Nine;
        public const int Eight = (int)Ranks.Eight;
        public const int Seven = (int)Ranks.Seven;
        public const int Six = (int)Ranks.Six;
        public const int Five = (int)Ranks.Five;
        public const int Four = (int)Ranks.Four;
        public const int Three = (int)Ranks.Three;
        public const int Two = (int)Ranks.Two;

        public static Ranks From(char value)
        {
            switch (value)
            {
                case '2':
                    return Ranks.Two;
                case '3':
                    return Ranks.Three;
                case '4':
                    return Ranks.Four;
                case '5':
                    return Ranks.Five;
                case '6':
                    return Ranks.Six;
                case '7':
                    return Ranks.Seven;
                case '8':
                    return Ranks.Eight;
                case '9':
                    return Ranks.Nine;
                case 't':
                case 'T':
                    return Ranks.Ten;
                case 'b':
                case 'j':
                case 'B':
                case 'J':
                    return Ranks.Jack;
                case 'q':
                case 'v':
                case 'Q':
                case 'V':
                    return Ranks.Queen;
                case 'h':
                case 'k':
                case 'H':
                case 'K':
                    return Ranks.King;
                case 'a':
                case 'A':
                    return Ranks.Ace;
                default:
                    throw new FatalBridgeException(string.Format("RankConverter.From(char): unknown rank: {0}", value));
            }
        }

        public static Ranks From(string value)
        {
            switch (value)
            {
                case "Two":
                    return Ranks.Two;
                case "Three":
                    return Ranks.Three;
                case "Four":
                    return Ranks.Four;
                case "Five":
                    return Ranks.Five;
                case "Six":
                    return Ranks.Six;
                case "Seven":
                    return Ranks.Seven;
                case "Eight":
                    return Ranks.Eight;
                case "Nine":
                    return Ranks.Nine;
                case "Ten":
                    return Ranks.Ten;
                case "Jack":
                    return Ranks.Jack;
                case "Queen":
                    return Ranks.Queen;
                case "King":
                    return Ranks.King;
                case "Ace":
                    return Ranks.Ace;
                default:
                    return From(value.Trim()[0]);
            }
        }

        public static string ToXML(this Ranks value)
        {
            switch (value)
            {
                case Ranks.Two:
                    return "2";
                case Ranks.Three:
                    return "3";
                case Ranks.Four:
                    return "4";
                case Ranks.Five:
                    return "5";
                case Ranks.Six:
                    return "6";
                case Ranks.Seven:
                    return "7";
                case Ranks.Eight:
                    return "8";
                case Ranks.Nine:
                    return "9";
                case Ranks.Ten:
                    return "T";
                case Ranks.Jack:
                    return "J";
                case Ranks.Queen:
                    return "Q";
                case Ranks.King:
                    return "K";
                case Ranks.Ace:
                    return "A";
                default:
                    throw new FatalBridgeException(string.Format("RankConverter.ToXML: unknown rank: {0}", value));
            }
        }

        public static string ToXML(this VirtualRanks value)
        {
            return ToXML((Ranks)value);
        }

        /// <summary>
        /// Localized representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Localized representation</returns>
        public static string ToText(this Ranks value)
        {
            switch (value)
            {
                case Ranks.Two:
                    return "2";
                case Ranks.Three:
                    return "3";
                case Ranks.Four:
                    return "4";
                case Ranks.Five:
                    return "5";
                case Ranks.Six:
                    return "6";
                case Ranks.Seven:
                    return "7";
                case Ranks.Eight:
                    return "8";
                case Ranks.Nine:
                    return "9";
                case Ranks.Ten:
                    return LocalizationResources.Ten.Substring(0, 1);
                case Ranks.Jack:
                    return LocalizationResources.Jack.Substring(0, 1);
                case Ranks.Queen:
                    return LocalizationResources.Queen.Substring(0, 1);
                case Ranks.King:
                    return LocalizationResources.King.Substring(0, 1);
                case Ranks.Ace:
                    return LocalizationResources.Ace.Substring(0, 1);
                default:
                    throw new FatalBridgeException(string.Format("RankConverter.ToText: unknown rank: {0}", value));
            }
        }

        public static int HCP(this Ranks value)
        {
            return (value >= Ranks.Jack ? (int)value - 8 : 0);
        }
    }

    public class RankCollection<T>
    {
        private T[] x = new T[13];

        public RankCollection(T initialValue)
        {
            for (Ranks s = Ranks.Two; s <= Ranks.Ace; s++)
                this[s] = initialValue;
        }

        public RankCollection(T[] initialValues)
        {
            for (Ranks s = Ranks.Two; s <= Ranks.Ace; s++)
                this[s] = initialValues[(int)s];
        }

        public T this[Ranks index]
        {
            get
            {
                return x[(int)index];
            }
            set
            {
                x[(int)index] = value;
            }
        }
    }
}
