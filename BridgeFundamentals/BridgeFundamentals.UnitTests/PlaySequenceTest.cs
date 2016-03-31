using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sodes.Bridge.Base.Test
{
	[TestClass]
	public class PlaySequenceTest
	{
		[TestMethod, TestCategory("CI"), TestCategory("Other")]
		public void PlaySequence_Record()
		{
			var target = new PlaySequence(new Contract("1NT", Seats.South, Vulnerable.Neither), 13, Seats.West);

			Assert.AreEqual<Seats>(Seats.West, target.whoseTurn, "");
		}
	}
}
