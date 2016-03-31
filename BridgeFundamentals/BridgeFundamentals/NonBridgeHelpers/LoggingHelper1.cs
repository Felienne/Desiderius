
#define DEBUG
// trick for Trace in PCL

using System;

namespace Sodes.Base
{
    public static partial class Log
    {
        public static int Level { get; set; }

        public static void Trace(int level, string message, params object[] args)
        {
            if (level <= Log.Level)
            {
                var msg = string.Format(message, args);
                msg = string.Format("{0:HH:mm:ss.fff} {1}", DateTime.UtcNow, msg);
                System.Diagnostics.Debug.WriteLine(msg);
            }
        }
    }
}
