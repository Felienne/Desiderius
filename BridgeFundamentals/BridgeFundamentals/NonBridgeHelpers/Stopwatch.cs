using System;

namespace Sodes.Base
{
	public class Stopwatch
	{
		private DateTime startTime;
		private long elapsedTicks;
		private bool isRunning = false;
		private object locker = new object();

		public TimeSpan Elapsed
		{
			get
			{
				if (this.IsRunning)
				{
					return new TimeSpan(this.elapsedTicks + DateTime.UtcNow.Subtract(this.startTime).Ticks);
				}
				else
				{
					return new TimeSpan(this.elapsedTicks);
				}
			}
		}

		public void Start()
		{
			this.startTime = DateTime.UtcNow;
			this.IsRunning = true;
		}

		public void Reset()
		{
			this.Start();
			this.elapsedTicks = 0;
		}

		public void Stop()
		{
			if (this.IsRunning)
			{
				var stopTime = DateTime.UtcNow;
				this.IsRunning = false;
				this.elapsedTicks += stopTime.Subtract(this.startTime).Ticks;
			}
		}

		public bool IsRunning 
		{ 
			get 
			{
				lock (this.locker)
				{
					return this.isRunning;
				}
			}
			set 
			{
				lock (this.locker)
				{
					this.isRunning = value;
				}
			}
		}
	}
}
