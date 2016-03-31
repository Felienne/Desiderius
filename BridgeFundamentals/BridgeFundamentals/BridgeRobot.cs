using Sodes.Base;
using System;

namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Base for the robot that has to implement bidding and playing tactics.
    /// </summary>
    public abstract class BridgeRobot : BoardResultOwner
    {
        public BridgeRobot(Seats seat) : this(seat, null)
        {
        }

        public BridgeRobot(Seats seat, BridgeEventBus bus) : base("BridgeRobot." + seat.ToXMLFull(), bus)
        {
            this.mySeat = seat;
        }

        private Seats mySeat;

        public abstract Bid FindBid(Bid lastRegularBid, bool allowDouble, bool allowRedouble);

        public abstract Card FindCard(Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick);

        #region Bridge Event Handlers

        public override void HandleCardPosition(Seats seat, Suits suit, Ranks rank)
        {
            // no cheating: only look at the card when it is meant for you
            if (seat == this.mySeat
                || (this.CurrentResult.Auction.Ended
                    && seat == this.CurrentResult.Play.Dummy
                    )
                )
            {
                base.HandleCardPosition(seat, suit, rank);
            }
        }

        public override void HandleBidNeeded(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble)
        {
            if (this.CurrentResult.Distribution.Length(this.mySeat) < 13) throw new InvalidOperationException("no cards");
            if (whoseTurn == this.mySeat && this.EventBus != null)
            {
                var myBid = this.FindBid(lastRegularBid, allowDouble, allowRedouble);
                //Log.Trace("BridgeRobot({0}).HandleBidNeeded: bids {1}", whoseTurn.ToString().PadRight(5), myBid);
                this.EventBus.HandleBidDone(this.mySeat, myBid);
            }
        }

        public override void HandleCardNeeded(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
        {
            if (whoseTurn != this.CurrentResult.Play.whoseTurn)
                throw new ArgumentOutOfRangeException("whoseTurn", "Expected a needcard from " + this.CurrentResult.Play.whoseTurn);

            if (controller == this.mySeat && this.EventBus != null)
            {
                var myCard = this.FindCard(whoseTurn, leadSuit, trump, trumpAllowed, leadSuitLength, trick);
                //Log.Trace("BridgeRobot({2}).HandleCardNeeded: {0} plays {3}{1}", whoseTurn.ToString().PadRight(5), myCard.Suit.ToXML(), this.mySeat.ToString().PadRight(5), myCard.Rank.ToXML());
                this.EventBus.HandleCardPlayed(whoseTurn, myCard.Suit, myCard.Rank);
            }
        }

        #endregion
    }
}
