using System;
using System.Collections.Generic;

namespace Sodes.Base
{
	public abstract class Deployment
	{
		public string ConventionCards { get; set; }

		public string OfflineTournaments { get; set; }

		public abstract string[] GetFiles(string path, string searchPattern);

        public abstract void EnsureSufficientExecutionStack2();

		public static Deployment Instance { get; protected set; }
	}
}
