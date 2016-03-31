using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sodes.Bridge.Base.Test
{
	[TestClass]
	public class AuctionTest
	{
		[TestMethod, TestCategory("CI"), TestCategory("Bid")]
		public void Auction_RecordOk1()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			Assert.AreEqual<Vulnerable>(Vulnerable.EW, target.Vulnerability, "Vulnerability");
			Assert.AreEqual<Seats>(Seats.East, target.Dealer, "Dealer");
			Assert.AreEqual<Seats>(Seats.East, target.WhoseTurn, "WhoseTurn");

			target.Record(Bid.C("p"));

			Assert.AreEqual<Seats>(Seats.South, target.WhoseTurn, "WhoseTurn 1");
			Assert.AreEqual<Seats>(Seats.East, target.WhoBid0(0), "WhoBid0 1");
			Assert.AreEqual<Seats>(Seats.East, target.WhoBid(1), "WhoBid 1");

			target.Record(Bid.C("1NT"));

			Assert.AreEqual<Seats>(Seats.West, target.WhoseTurn, "WhoseTurn 2");
			Assert.AreEqual<Seats>(Seats.South, target.WhoBid0(1), "WhoBid0 2");
			Assert.AreEqual<Seats>(Seats.East, target.WhoBid0(0), "WhoBid0 2a");
			Assert.AreEqual<Seats>(Seats.South, target.WhoBid(1), "WhoBid 2");
			Assert.AreEqual<Seats>(Seats.East, target.WhoBid(2), "WhoBid 2a");

			target.Record(Bid.C("x"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));

			Assert.AreEqual<Seats>(Seats.South, target.WhoseTurn, "WhoseTurn 5");

			target.Record(Bid.C("p"));

			Assert.AreEqual<Seats>(Seats.West, target.WhoseTurn, "WhoseTurn after end of bidding");
			Assert.AreEqual<Seats>(Seats.South, target.Declarer, "Declarer after end of bidding");
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid")]
		public void Auction_RecordOk2()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("x"));
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid")]
		public void Auction_RecordOk3()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("x"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid")]
		public void Auction_RecordOk4()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("x"));
			target.Record(Bid.C("xx"));
			target.Record(Bid.C("2C"));
			target.Record(Bid.C("x"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault1()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("x"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault2()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("x"));
			target.Record(Bid.C("x"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault3()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("x"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault4()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("x"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault5()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault6()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), ExpectedException(typeof(AuctionException))]
		public void Auction_RecordFault7()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("1NT"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("xx"));
		}

		[TestMethod, TestCategory("CI"), TestCategory("Bid"), TestCategory("CI"), TestCategory("Bid")]
		public void Auction_Declarer_4Pass()
		{
			var target = new Auction(Vulnerable.EW, Seats.East);
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			target.Record(Bid.C("p"));
			Assert.AreEqual<Seats>(Seats.East, target.Declarer);
		}
	}
}
