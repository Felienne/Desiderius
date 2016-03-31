using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Sodes.Base.Test;

namespace Sodes.Base.Test.Helpers
{
	public class TestBase
	{
		private TestContext testContextInstance;

		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
	}
}
