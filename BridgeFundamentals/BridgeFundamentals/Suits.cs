#define faster
using System;
using System.Diagnostics;

namespace Sodes.Bridge.Base
{
    public class ParserString { }  // om in type convert voor suit te kunnen omzetten naar K,R,H,S

    public enum Suits
    { Clubs, Diamonds, Hearts, Spades, NoTrump }

    public static class SuitHelper
    {
        public const int Clubs = (int)Suits.Clubs;
        public const int Diamonds = (int)Suits.Diamonds;
        public const int Hearts = (int)Suits.Hearts;
        public const int Spades = (int)Suits.Spades;
        public const int NoTrump = (int)Suits.NoTrump;

        [DebuggerStepThrough]
        public static Suits FromXML(string value)
        {
            return FromXML(value[0]);
        }

        [DebuggerStepThrough]
        public static Suits FromXML(char value)
        {
            switch (value)
            {
                case 'C':
                case 'c':
                case 'K':
                case 'k':
                    return Suits.Clubs;
                case 'D':
                case 'd':
                case 'R':
                case 'r':
                    return Suits.Diamonds;
                case 'H':
                case 'h':
                    return Suits.Hearts;
                case 'S':
                case 's':
                    return Suits.Spades;
                case 'N':
                case 'n':
                case 'Z':
                case 'z':
                    return Suits.NoTrump;
                default:
                    throw new FatalBridgeException(string.Format("SuitConverter.FromXML: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Convert to special Unicode characters that represent the symbols for clubs, diamonds, hearts and spades
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Unicode chacracter</returns>
        [DebuggerStepThrough]
        public static Char ToUnicode(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs:
                    return System.Convert.ToChar(9827);
                case Suits.Diamonds:
                    return System.Convert.ToChar(9830);
                case Suits.Hearts:
                    return System.Convert.ToChar(9829);
                case Suits.Spades:
                    return System.Convert.ToChar(9824);
                case Suits.NoTrump:
                    return 'N';
                default:
                    throw new FatalBridgeException(string.Format("SuitConverter.ToUnicode: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Convert to XML representation of suit
        /// </summary>
        /// <param name="value"></param>
        /// <returns>"C", "D", "H", "S" or "NT"</returns>
        [DebuggerStepThrough]
        public static string ToXML(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs: return "C";
                case Suits.Diamonds: return "D";
                case Suits.Hearts: return "H";
                case Suits.Spades: return "S";
                case Suits.NoTrump: return "NT";
                default:
                    throw new FatalBridgeException(string.Format("SuitConverter.ToXML: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Convert to character that will be recognized by the rule parser.
        /// Exception for NT
        /// </summary>
        /// <param name="value"></param>
        /// <returns>"C", "D", "H" or "S"</returns>
        [DebuggerStepThrough]
        public static string ToParser(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs: return "C";
                case Suits.Diamonds: return "D";
                case Suits.Hearts: return "H";
                case Suits.Spades: return "S";
                case Suits.NoTrump: return "N";
                default:
                    throw new FatalBridgeException(string.Format("SuitConverter.ToParser: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Convert to localized representation of suit
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Localized representation</returns>
        [DebuggerStepThrough]
        public static string ToString(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs: return LocalizationResources.Clubs.Substring(0, 1);
                case Suits.Diamonds: return LocalizationResources.Diamonds.Substring(0, 1);
                case Suits.Hearts: return LocalizationResources.Hearts.Substring(0, 1);
                case Suits.Spades: return LocalizationResources.Spades.Substring(0, 1);
                case Suits.NoTrump: return LocalizationResources.NoTrump;
                default:
                    throw new FatalBridgeException(string.Format("SuitConverter.ToString: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// next suit
        /// </summary>
        [DebuggerStepThrough]
        public static Suits Next(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs: return Suits.Diamonds;
                case Suits.Diamonds: return Suits.Hearts;
                case Suits.Hearts: return Suits.Spades;
                case Suits.Spades: return Suits.Clubs;
                default:
                    throw new FatalBridgeException(string.Format("Suits.Next: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Previous suit
        /// </summary>
        [DebuggerStepThrough]
        public static Suits Previous(this Suits value)
        {
            switch (value)
            {
                case Suits.Clubs: return Suits.Spades;
                case Suits.Diamonds: return Suits.Clubs;
                case Suits.Hearts: return Suits.Diamonds;
                case Suits.Spades: return Suits.Hearts;
                default:
                    throw new FatalBridgeException(string.Format("Suits.Previous: unknown suit: {0}", value));
            }
        }

        /// <summary>
        /// Is it hearts or spades?
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsMajor(this Suits value)
        {
            return value == Suits.Hearts || value == Suits.Spades;
        }

        /// <summary>
        /// Is it diamonds or clubs?
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsMinor(this Suits value)
        {
            return value == Suits.Clubs || value == Suits.Diamonds;
        }

        [DebuggerStepThrough]
        public static void ForEachSuit(Action<Suits> toDo)
        {
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                toDo(s);
            }
        }

        [DebuggerStepThrough]
        public static void ForEachTrump(Action<Suits> toDo)
        {
            for (Suits s = Suits.Clubs; s <= Suits.NoTrump; s++)
            {
                toDo(s);
            }
        }

        [DebuggerStepThrough]
        public static void ForEachMajor(Action<Suits> toDo)
        {
            for (Suits s = Suits.Hearts; s <= Suits.Spades; s++)
            {
                toDo(s);
            }
        }

        [DebuggerStepThrough]
        public static void ForEachMinor(Action<Suits> toDo)
        {
            for (Suits s = Suits.Clubs; s.IsMinor(); s++)
            {
                toDo(s);
            }
        }
    }

    public class SuitCollection<T>
    {
        private T[] x = new T[5];

        public SuitCollection()
        {
        }

        public SuitCollection(T initialValue)
        {
            this.Set(initialValue);
        }

        public SuitCollection(T[] initialValues)
        {
            for (Suits s = Suits.Clubs; s <= Suits.NoTrump; s++)
                this[s] = initialValues[(int)s];
        }

        public T this[Suits index]
        {
            [DebuggerStepThrough]
            get
            {
                return x[(int)index];
            }
            [DebuggerStepThrough]
            set
            {
                x[(int)index] = value;
            }
        }

        public T this[int index]
        {
            [DebuggerStepThrough]
            get
            {
                return x[index];
            }
            [DebuggerStepThrough]
            set
            {
                x[index] = value;
            }
        }

        public void Set(T value)
        {
            for (Suits s = Suits.Clubs; s <= Suits.NoTrump; s++)
                this[s] = value;
        }

        //public SuitCollection<T> Clone()
        //{
        //  SuitCollection<T> result = new SuitCollection<T>();
        //  for (Suits s = Suits.Clubs; s <= Suits.NoTrump; s++)
        //  {
        //    result[s] = this[s];
        //  }

        //  return result;
        //}
    }

    /// <summary>
    /// This specific version of a SuitRankCollection is a fraction faster in cloning, uses bytes to store data while allowing int in the interface
    /// </summary>
    public class SuitRankCollectionInt
    {
        /// Benchmark figures:
        /// for SuitRankCollectionInt.SuitRankCollection<int>, SuitRankCollection<short>
        // 2,7711585
        // 2,7671582
        // Clone:0,2350135
        // 2,7511574
        // 2,7461571
        // Clone:0,4110235
        // 2,9401681
        // 2,8711642
        // Clone:0,3520202


        private byte[] x = new byte[52];

        public SuitRankCollectionInt()
        {
        }

        public SuitRankCollectionInt(int initialValue)
            : this()
        {
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                this.Init(s, initialValue);
            }
        }

        public int this[Suits suit, Ranks rank]
        {
            get
            {
                return x[13 * (int)suit + (int)rank];
            }
            set
            {
#if DEBUG
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException();
#endif
                x[13 * (int)suit + (int)rank] = (byte)value;
            }
        }

        public int this[int suit, int rank]
        {
            get
            {
                return x[13 * suit + rank];
            }
            set
            {
#if DEBUG
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException();
#endif
                x[13 * suit + rank] = (byte)value;
            }
        }

        public void Init(Suits suit, int value)
        {
            int _s = 13 * (int)suit;
            for (int r = Rank.Two; r <= Rank.Ace; r++)
            {
#if DEBUG
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException();
#endif
                this.x[_s + r] = (byte)value;
            }
        }

        public SuitRankCollectionInt Clone()
        {
            SuitRankCollectionInt result = new SuitRankCollectionInt();
            System.Buffer.BlockCopy(this.x, 0, result.x, 0, 52);
            return result;
        }
    }

    public class SuitRankCollection<T>
    {
        private T[] x = new T[52];
        private int typeSize = -1;

        public SuitRankCollection()
        {
            var typeName = typeof(T).Name;
            if (typeName == "Int32") typeSize = 4 * 52;
            else if (typeName == "Int16") typeSize = 2 * 52;
            else if (typeName == "Byte") typeSize = 1 * 52;
        }

        public SuitRankCollection(T initialValue)
            : this()
        {
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                this.Init(s, initialValue);
            }
        }

        public T this[Suits suit, Ranks rank]
        {
            get
            {
                return x[13 * (int)suit + (int)rank];
            }
            set
            {
                x[13 * (int)suit + (int)rank] = value;
            }
        }

        public T this[int suit, int rank]
        {
            get
            {
                return x[13 * suit + rank];
            }
            set
            {
                x[13 * suit + rank] = value;
            }
        }

        private void Init(Suits suit, T value)
        {
            int _s = 13 * (int)suit;
            for (int r = Rank.Two; r <= Rank.Ace; r++)
            {
                this.x[_s + r] = value;
            }
        }

        public SuitRankCollection<T> Clone()
        {
            SuitRankCollection<T> result = new SuitRankCollection<T>();

            if (this.typeSize> 0)
            {
                System.Buffer.BlockCopy(this.x, 0, result.x, 0, typeSize);
            }
            else
            {
                //this.x.CopyTo(result.x, 0);
                //Array.Copy(this.x, result.x, 52);
                for (int s = 0; s <= 3; s++)
                {
                    int _s = 13 * s;
                    for (int r = 0; r <= 12; r++)
                    {
                        int i = _s + r;
                        result.x[i] = this.x[i];
                    }
                }
            }

            return result;
        }
    }
}
