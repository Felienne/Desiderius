using Sodes.Bridge.Base;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Bridge.Base.Test.Helpers;

namespace BridgeFundamentals.UnitTests
{
    public class TestRobot : BridgeRobot
    {
        public TestRobot(Seats seat) : this(seat, null)
        {
        }

        public TestRobot(Seats seat, BridgeEventBus bus) : base(seat, bus)
        {
        }

        public override Bid FindBid(Bid lastRegularBid, bool allowDouble, bool allowRedouble)
        {
            /// this is just some basic logic to enable testing
            /// override this method and implement your own logic
            /// 
            if (lastRegularBid.IsPass) return Bid.C("1NT");
            return Bid.C("Pass");
        }

        public override Card FindCard(Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
        {
            if (leadSuit == Suits.NoTrump || leadSuitLength == 0)
            {   // 1st man or void in lead suit
                for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                    {
                        if (this.CurrentResult.Distribution.Owns(whoseTurn, s, r))
                        {
                            return new Card(s, r);
                        }
                    }
                }
            }
            else
            {
                for (Ranks r = Ranks.Two; r <= Ranks.Ace; r++)
                {
                    if (this.CurrentResult.Distribution.Owns(whoseTurn, leadSuit, r))
                    {
                        return new Card(leadSuit, r);
                    }
                }
            }

            throw new InvalidOperationException("BridgeRobot.FindCard: no card found");
        }
    }

    [TestClass]
    public class TestRobotTest : BridgeTestBase
    {
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            BridgeTestBase.Initialize(testContext);
        }

        [TestMethod]
        public void TestRobot_Handle1Board()
        {
            var r = new TestRobot(Seats.North);
            r.HandleTournamentStarted(Scorings.scPairs, 120, 1, "");
            r.HandleRoundStarted(null, new DirectionDictionary<string>("", ""));
            r.HandleBoardStarted(1, Seats.North, Vulnerable.Neither);
            r.HandleCardPosition(Seats.North, Suits.Clubs, Ranks.Ace);
            r.HandleCardPosition(Seats.North, Suits.Clubs, Ranks.King);
            r.HandleCardPosition(Seats.North, Suits.Clubs, Ranks.Nine);
            r.HandleCardPosition(Seats.North, Suits.Clubs, Ranks.Seven);
            r.HandleCardPosition(Seats.North, Suits.Diamonds, Ranks.King);
            r.HandleCardPosition(Seats.North, Suits.Diamonds, Ranks.Three);
            r.HandleCardPosition(Seats.North, Suits.Hearts, Ranks.Ace);
            r.HandleCardPosition(Seats.North, Suits.Hearts, Ranks.Six);
            r.HandleCardPosition(Seats.North, Suits.Hearts, Ranks.Five);
            r.HandleCardPosition(Seats.North, Suits.Hearts, Ranks.Three);
            r.HandleCardPosition(Seats.North, Suits.Spades, Ranks.Ace);
            r.HandleCardPosition(Seats.North, Suits.Spades, Ranks.Queen);
            r.HandleCardPosition(Seats.North, Suits.Spades, Ranks.Eight);
            r.HandleBidDone(Seats.North, r.FindBid(Bid.C("Pass"), false, false));
            r.HandleBidDone(Seats.East, Bid.C("Pass"));
            r.HandleBidDone(Seats.South, Bid.C("Pass"));
            r.HandleBidDone(Seats.West, Bid.C("Pass"));
            r.HandleAuctionFinished(Seats.North, new Contract("1NT", Seats.North, Vulnerable.Neither));
            r.HandleCardPlayed(Seats.East, Suits.Diamonds, Ranks.Queen);
            r.HandleShowDummy(Seats.South);
            r.HandleCardPlayed(Seats.South, Suits.Diamonds, Ranks.Two);
            r.HandleCardPlayed(Seats.West, Suits.Diamonds, Ranks.Four);
            r.HandleCardNeeded(Seats.North, Seats.North, Suits.Diamonds, Suits.NoTrump, false, 2, 1);
            var card = r.FindCard(Seats.North, Suits.Diamonds, Suits.NoTrump, false, 2, 1);
        }
    }
}
