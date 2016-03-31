using System;

namespace Sodes.Bridge.Base
{
    public class BoardResultOwner : BridgeEventBusClient
    {

        public BoardResultOwner(string _owner, BridgeEventBus bus) : base(bus, _owner)
        {
            this.Owner = _owner;
        }

        private string Owner;
        protected BoardResultRecorder CurrentResult;

        protected virtual BoardResultRecorder NewBoardResult(int boardNumber)
        {
            return new BoardResultRecorder(this.Owner + ".Result." + boardNumber, null);
        }

        #region Bridge Event Handlers

        public override void HandleBoardStarted(int boardNumber, Seats dealer, Vulnerable vulnerabilty)
        {
            base.HandleBoardStarted(boardNumber, dealer, vulnerabilty);
            this.CurrentResult = NewBoardResult(boardNumber);
            this.CurrentResult.HandleBoardStarted(boardNumber, dealer, vulnerabilty);
        }

        public override void HandleCardPosition(Seats seat, Suits suit, Ranks rank)
        {
            base.HandleCardPosition(seat, suit, rank);
            this.CurrentResult.HandleCardPosition(seat, suit, rank);
        }

        public override void HandleCardDealingEnded()
        {
            base.HandleCardDealingEnded();
            this.CurrentResult.HandleCardDealingEnded();
        }

        public override void HandleBidNeeded(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble)
        {
            base.HandleBidNeeded(whoseTurn, lastRegularBid, allowDouble, allowRedouble);
            this.CurrentResult.HandleBidNeeded(whoseTurn, lastRegularBid, allowDouble, allowRedouble);
        }

        public override void HandleBidDone(Seats source, Bid bid)
        {
            base.HandleBidDone(source, bid);
            this.CurrentResult.HandleBidDone(source, bid);
        }

        public override void HandleAuctionFinished(Seats declarer, Contract finalContract)
        {
            base.HandleAuctionFinished(declarer, finalContract);
            this.CurrentResult.HandleAuctionFinished(declarer, finalContract);
        }

        public override void HandleCardNeeded(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
        {
            base.HandleCardNeeded(controller, whoseTurn, leadSuit, trump, trumpAllowed, leadSuitLength, trick);
            this.CurrentResult.HandleCardNeeded(controller, whoseTurn, leadSuit, trump, trumpAllowed, leadSuitLength, trick);
        }

        public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
        {
            base.HandleCardPlayed(source, suit, rank);
            this.CurrentResult.HandleCardPlayed(source, suit, rank);
        }

        public override void HandleNeedDummiesCards(Seats dummy)
        {
            base.HandleNeedDummiesCards(dummy);
            this.CurrentResult.HandleNeedDummiesCards(dummy);
        }

        public override void HandleShowDummy(Seats dummy)
        {
            base.HandleShowDummy(dummy);
            this.CurrentResult.HandleShowDummy(dummy);
        }

        public override void HandleTrickFinished(Seats trickWinner, int tricksForDeclarer, int tricksForDefense)
        {
            base.HandleTrickFinished(trickWinner, tricksForDeclarer, tricksForDefense);
            this.CurrentResult.HandleTrickFinished(trickWinner, tricksForDeclarer, tricksForDefense);
        }

        public override void HandlePlayFinished(BoardResultRecorder currentResult)
        {
            base.HandlePlayFinished(currentResult);
            this.CurrentResult.HandlePlayFinished(currentResult);
            //this.CurrentResult = null;
        }

        #endregion
    }
}
