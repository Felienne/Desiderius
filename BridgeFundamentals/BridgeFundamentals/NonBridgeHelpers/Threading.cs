using System.Threading;

namespace Sodes.Base
{
	public static class Threading
	{
		/// <summary>
		/// Portable version of Thread.Sleep
		/// </summary>
		/// <param name="milliSeconds">Time to pause</param>
		public static void Sleep(int milliSeconds)
		{
			using (var mre = new ManualResetEvent(false))
			{
				mre.WaitOne(milliSeconds);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minMilliSeconds">Minimum time to sleep</param>
		/// <param name="maxMilliseconds">Maximum time to sleep</param>
		public static void SleepRandom(int minMilliSeconds, int maxMilliseconds)
		{
			Sleep(minMilliSeconds + RandomGenerator.Next(maxMilliseconds - minMilliSeconds));
		}
	}
}
