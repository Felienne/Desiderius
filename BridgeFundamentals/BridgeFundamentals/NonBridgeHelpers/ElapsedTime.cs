using System;

namespace Sodes.Base
{
    public static class ElapsedTime
    {
        public static double Do(Action y, int loopSize = 1)
        {
#if DEBUG
            if (y == null) throw new ArgumentNullException("y");
#endif
            var startTime = DateTime.UtcNow;
            {
                for (int i = 0; i < loopSize; i++)
                {
                    y();
                }
            }
            var endTime = DateTime.UtcNow;
            TimeSpan duration = endTime - startTime;
            return duration.TotalSeconds / loopSize;
        }

        public static string ProfileTime(string myEvent)
        {
            return "============ " + DateTime.UtcNow.ToString("hh:mm:ss.f") + " " + myEvent;
        }
    }
}
