using System;
using System.Diagnostics;

namespace Sodes.Base
{
    public static partial class Log
    {
        [Conditional("DEBUG")]
        public static void Debug(string message, params object[] args)
        {
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.f} {1}", DateTime.UtcNow, msg);
        }
    }
}
