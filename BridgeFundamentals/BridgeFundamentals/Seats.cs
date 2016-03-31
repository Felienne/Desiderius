using System.Collections.Generic;   // IEnumerator<T>
using System.Diagnostics;
using System.Runtime.Serialization;
using System;

namespace Sodes.Bridge.Base
{
    public enum Seats
    { North, East, South, West }

    public static class SeatsExtensions
    {
        /// <summary>Seat that comes next to specified seat</summary>
        /// <param name="x">Seat for which to find the next seat</param>
        /// <returns>The next seat</returns>
        [DebuggerStepThrough]
        public static Seats Next(this Seats x)
        {
            return (Seats)((1 + (int)x) % 4);
        }

        /// <summary>Seat that comes before the specified seat</summary>
        /// <param name="x">Seat for which to find the previous seat</param>
        /// <returns>The previous seat</returns>
        [DebuggerStepThrough]
        public static Seats Previous(this Seats x)
        {
            return (Seats)((3 + (int)x) % 4);
        }

        /// <summary>Seat that partners with the specified seat</summary>
        /// <param name="x">Seat for which to find the partner</param>
        /// <returns>The partner seat</returns>
        [DebuggerStepThrough]
        public static Seats Partner(this Seats x)
        {
            return (Seats)((2 + (int)x) % 4);
        }

        [DebuggerStepThrough]
        public static Seats FromXML(string value)
        {
            switch (value.Substring(0, 1).ToUpperInvariant())
            {
                case "N":
                    return Seats.North;
                case "E":
                case "O":
                    return Seats.East;
                case "S":
                case "Z":
                    return Seats.South;
                case "W":
                    return Seats.West;
                default:
                    throw new FatalBridgeException("Unknown seat: " + value);
            }
        }

        [DebuggerStepThrough]
        public static Seats DealerFromBoardNumber(int boardNumber)
        {
            int board = ((boardNumber - 1) % 4);
            return (Seats)board;
        }

        [DebuggerStepThrough]
        public static string ToXML(this Seats value)
        {
            switch (value)
            {
                case Seats.North: return "N";
                case Seats.East: return "E";
                case Seats.South: return "S";
                default: return "W";
            }
        }

        [DebuggerStepThrough]
        public static string ToXMLFull(this Seats value)
        {
            switch (value)
            {
                case Seats.North: return "North";
                case Seats.East: return "East";
                case Seats.South: return "South";
                default: return "West";
            }
        }

        /// <summary>
        /// Localized string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ToString2(this Seats value)
        {
            switch (value)
            {
                case Seats.North: return LocalizationResources.North;
                case Seats.East: return LocalizationResources.East;
                case Seats.South: return LocalizationResources.South;
                default: return LocalizationResources.West;
            }
        }

        [DebuggerStepThrough]
        public static Directions Direction(this Seats x)
        {
            switch (x)
            {
                case Seats.North:
                case Seats.South: return Directions.NorthSouth;
                case Seats.East:
                case Seats.West: return Directions.EastWest;
                default: return Directions.NorthSouth;    // voor de compiler
            }
        }

        [DebuggerStepThrough]
        public static bool IsSameDirection(this Seats s1, Seats s2)
        {
            return s1.Direction() == s2.Direction();
        }

        [DebuggerStepThrough]
        public static void ForEachSeat(Action<Seats> toDo)
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                toDo(s);
            }
        }
    }

    public enum Directions { NorthSouth, EastWest }

    [DebuggerDisplay("{values}")]
    [DataContract]
    public class SeatCollection<T>
    {
        [DataMember]
        private Dictionary<Seats, T> values = new Dictionary<Seats, T>();

        public SeatCollection()
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                this[s] = default(T);
            }
        }

        public SeatCollection(T[] initialValue)
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                this[s] = initialValue[(int)s];
            }
        }

        [IgnoreDataMember]
        public T this[Seats index]
        {
            get
            {
                return values[index];
            }
            set
            {
                values[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
                if (this[s] != null)
                    yield return this[s];
        }
    }

    public class DirectionDictionary<T> : Dictionary<Directions, T>
    {
        public DirectionDictionary(T valueNorthSouth, T valueEastWest)
        {
            this[Directions.NorthSouth] = valueNorthSouth;
            this[Directions.EastWest] = valueEastWest;
        }
    }
}
