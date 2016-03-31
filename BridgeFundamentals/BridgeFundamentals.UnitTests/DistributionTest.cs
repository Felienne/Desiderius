using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Bridge.Base;

namespace TestCommonBridge
{
	[TestClass]
	public class DistributionTest
	{
		[TestMethod, TestCategory("CI"), TestCategory("Other")]
		public void Distribution_Clone_Test()
		{
			Distribution source = new Distribution();
			source.Give(Seats.North, Suits.Spades, Ranks.Seven);
			Distribution copy = source.Clone();
			copy.Played(Seats.North, Suits.Spades, Ranks.Seven);
			Assert.IsFalse(copy.Owns(Seats.North, Suits.Spades, Ranks.Seven), "weg uit copy");
			Assert.IsTrue(source.Owns(Seats.North, Suits.Spades, Ranks.Seven), "niet weg uit source");
		}
	}
}
