using System;

namespace Sodes.Base
{
    /// Math.Min is 9x faster than an own generic implementation!!!!
    /// So no need for a generic Math

    //public static class Math<T> where T : IComparable
    //{
    //    public static T Min(T a, T b, T c)
    //    {
    //        if (a.CompareTo(b) < 0 && a.CompareTo(c) < 0)
    //        {
    //            return a;
    //        }
    //        else
    //        {
    //            return b.CompareTo(c) < 0 ? b : c;
    //        }
    //    }
    //}
}
