using Sodes.Base;
using System;
using System.Threading.Tasks;

namespace Sodes.Bridge.Base
{
    public class TournamentController : BoardResultOwner
    {
        public Board2 currentBoard;
        private int boardNumber;
        private Tournament currentTournament;
        protected ParticipantInfo participant;
        private Action onTournamentFinished;

        public TournamentController(Tournament t, ParticipantInfo p) : this(t, p, null)
        {
        }

        public TournamentController(Tournament t, ParticipantInfo p, BridgeEventBus bus) : base("TournamentController", bus)
        {
            this.currentTournament = t;
            this.participant = p;
        }

        public async Task StartTournament(Action onTournamentFinish)
        {
            //Log.Trace("TournamentController2.StartTournament");
            this.boardNumber = 0;
            this.onTournamentFinished = onTournamentFinish;
            this.EventBus.HandleTournamentStarted(this.currentTournament.ScoringMethod, 120, this.participant.MaxThinkTime, this.currentTournament.EventName);
            this.EventBus.HandleRoundStarted(this.participant.PlayerNames.Names, new DirectionDictionary<string>(this.participant.ConventionCardNS, this.participant.ConventionCardWE));
            await this.NextBoard();
        }

        public void StartTournament()
        {
            //Log.Trace("TournamentController2.StartTournament");
            this.boardNumber = 0;
            this.EventBus.HandleTournamentStarted(this.currentTournament.ScoringMethod, 120, this.participant.MaxThinkTime, this.currentTournament.EventName);
            this.EventBus.HandleRoundStarted(this.participant.PlayerNames.Names, new DirectionDictionary<string>(this.participant.ConventionCardNS, this.participant.ConventionCardWE));
        }

        public async Task StartNextBoard()
        {
            //Log.Trace("TournamentController2.StartNextBoard");
            await this.NextBoard();
        }

        public override async void HandlePlayFinished(BoardResultRecorder currentResult)
        {
            //Log.Trace("TournamentController.HandlePlayFinished start");
            await this.currentTournament.SaveAsync(this.CurrentResult as BoardResult);
            //Log.Trace("TournamentController.HandlePlayFinished after SaveAsync");
            await this.NextBoard();
            //Log.Trace("TournamentController.HandlePlayFinished finished");
        }

        private async Task NextBoard()
        {
            //Log.Trace("TournamentController.NextBoard start");
            this.boardNumber++;
            this.currentBoard = await this.currentTournament.GetNextBoardAsync(this.boardNumber, this.participant.UserId);
            if (this.currentBoard == null)
            {
                //Log.Trace("TournamentController.NextBoard no next board");
                this.EventBus.HandleTournamentStopped();
                this.EventBus.Unlink(this);
                //Log.Trace("TournamentController.NextBoard after BridgeEventBus.MainEventBus.Unlink");
                if (this.onTournamentFinished != null) this.onTournamentFinished();
                //Log.Trace("TournamentController2.NextBoard after onTournamentFinished");
            }
            else
            {
                Log.Trace(1, "TournamentController.NextBoard board={0}", this.currentBoard.BoardNumber);
                this.EventBus.HandleBoardStarted(this.currentBoard.BoardNumber, this.currentBoard.Dealer, this.currentBoard.Vulnerable);
                foreach (var item in currentBoard.Distribution.Deal)
                {
                    this.EventBus.HandleCardPosition(item.Seat, item.Suit, item.Rank);
                }

                this.EventBus.HandleCardDealingEnded();
            }
        }

        protected override BoardResultRecorder NewBoardResult(int boardNumber)
        {
            return new BoardResultEventPublisher("TournamentController.Result." + currentBoard.BoardNumber, currentBoard, this.participant.PlayerNames.Names, this.EventBus);
        }
    }

    public class BoardResultEventPublisher : BoardResult
    {
        public BoardResultEventPublisher(string _owner, Board2 board, SeatCollection<string> newParticipants, BridgeEventBus bus)
            : base(_owner, board, newParticipants)
        {
            this.EventBus = bus;
        }

        private bool dummyVisible = false;
        protected BridgeEventBus EventBus;

        #region Bridge Event Handlers

        public override void HandleCardDealingEnded()
        {
            this.dummyVisible = false;
            base.HandleCardDealingEnded();
            //Log.Trace("BoardResultEventPublisher.HandleCardDealingEnded: 1st bid needed from {0}", this.Auction.WhoseTurn);
            this.EventBus.HandleBidNeeded(this.Auction.WhoseTurn, this.Auction.LastRegularBid, this.Auction.AllowDouble, this.Auction.AllowRedouble);
        }

        public override void HandleBidDone(Seats source, Bid bid)
        {
            //Log.Trace("BoardResultEventPublisher.HandleBidDone: {0} bids {1}", source, bid);

            base.HandleBidDone(source, bid);
            if (this.Auction.Ended)
            {
                //Log.Trace("BoardResultEventPublisher.HandleBidDone: auction finished");
                if (this.Contract.Bid.IsRegular)
                {
                    this.EventBus.HandleAuctionFinished(this.Auction.Declarer, this.Play.Contract);
                    this.NeedCard();
                }
                else
                {
                    //Log.Trace("BoardResultEventPublisher.HandleBidDone: all passed");
                    this.EventBus.HandlePlayFinished(this);
                }
            }
            else
            {
                //Log.Trace("BoardResultEventPublisher.HandleBidDone: next bid needed from {0}", this.Auction.WhoseTurn);
                this.EventBus.HandleBidNeeded(this.Auction.WhoseTurn, this.Auction.LastRegularBid, this.Auction.AllowDouble, this.Auction.AllowRedouble);
            }
        }

        public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
        {
            //Log.Trace("BoardResultEventPublisher({3}).HandleCardPlayed: {0} played {2}{1}", source, suit.ToXML(), rank.ToXML(), this.Owner);

            //if (!this.theDistribution.Owns(source, card))
            //  throw new FatalBridgeException(string.Format("{0} does not own {1}", source, card));
            /// 18-03-08: cannot check here: hosted tournaments get a card at the moment the card is played
            /// 

            if (this.Play == null)      // this is an event that is meant for the previous boardResult
                throw new ArgumentNullException("this.Play");

            if (source != this.Play.whoseTurn)
                throw new ArgumentOutOfRangeException("source", "Expected a card from " + this.Play.whoseTurn);

            base.HandleCardPlayed(source, suit, rank);
            if (this.Play.PlayEnded)
            {
                //Log.Trace("BoardResultEventPublisher({0}).HandleCardPlayed: play finished", this.Owner);
                this.EventBus.HandlePlayFinished(this);
            }
            else
            {
                if (!this.dummyVisible)
                {
                    this.dummyVisible = true;
                    this.EventBus.HandleNeedDummiesCards(this.Play.whoseTurn);
                }
                else if (this.Play.TrickEnded)
                {
                    this.EventBus.HandleTrickFinished(this.Play.whoseTurn, this.Play.Contract.tricksForDeclarer, this.Play.Contract.tricksForDefense);
                    this.NeedCard();
                }
                else
                {
                    this.NeedCard();
                }
            }
        }

        public override void HandleNeedDummiesCards(Seats dummy)
        {
            //Log.Trace("BoardResultEventPublisher({0}).HandleNeedDummiesCards", this.Name);
            if (this.Distribution.Length(dummy) == 13)
            {
                for (Suits suit2 = Suits.Spades; suit2 >= Suits.Clubs; suit2--)
                {
                    for (Ranks rank2 = Ranks.Ace; rank2 >= Ranks.Two; rank2--)
                    {
                        if (this.Distribution.Owns(dummy, suit2, rank2)) this.EventBus.HandleCardPosition(dummy, suit2, rank2);
                    }
                }

                this.EventBus.HandleShowDummy(dummy);
            }
            else
            {
                //Log.Trace("BoardResultEventPublisher({0}).HandleNeedDummiesCards waits for dummies cards", this.Name);
            }
        }

        public override void HandleShowDummy(Seats dummy)
        {
            base.HandleShowDummy(dummy);
            this.NeedCard();
        }

        #endregion

        private void NeedCard()
        {
            if (this.Auction == null) throw new ObjectDisposedException("this.theAuction");
            if (this.Play == null) throw new ObjectDisposedException("this.thePlay");

            Seats controller = this.Play.whoseTurn;
            if (this.Play.whoseTurn == this.Auction.Declarer.Partner())
            {
                controller = this.Auction.Declarer;
            }

            int leadSuitLength = this.Distribution.Length(this.Play.whoseTurn, this.Play.leadSuit);
            //Log.Trace("BoardResultEventPublisher({2}).NeedCard from {0} by {1}", this.Play.whoseTurn, controller, this.Name);
            this.EventBus.HandleCardNeeded(
                controller
                , this.Play.whoseTurn
                , this.Play.leadSuit
                , this.Play.Trump
                , leadSuitLength == 0 && this.Play.Trump != Suits.NoTrump
                , leadSuitLength
                , this.Play.currentTrick
            );
        }

    }

    public class ParticipantInfo
    {
        public Guid UserId { get; set; }
        public string ConventionCardNS { get; set; }
        public string ConventionCardWE { get; set; }
        public int MaxThinkTime { get; set; }
        public Participant PlayerNames { get; set; }
    }

    public delegate void TournamentFinishedHandler();

}
