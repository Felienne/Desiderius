using Sodes.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sodes.Bridge.Base
{
    public class BridgeEventBus : BridgeEventHandlers
    {
        public static BridgeEventBus MainEventBus = new BridgeEventBus("MainEventBus");

        public BridgeEventBus(string name)
        {
            this.eventBusName = name;
            Task.Run(async () =>
            {
                var delay = 5;
                while (true)
                {
                    while (this.work.Count > 0)
                    {
                        delay = 5;
                        Action workItem = null;
                        lock (this.work)
                        {
                            workItem = this.work.Dequeue();
                        }

                        try
                        {
                            workItem();
                        }
                        catch (Exception ex)
                        {
                            Log.Trace(0, ex.ToString());
                            throw;
                        }
                    }

                    await Task.Delay(delay);
                    if (delay < 10) delay *= 2;
                    //Log.Trace("BridgeEventBus {0} processing loop alive", this.eventBusName);
                }
            });
        }

        protected BridgeEventBus() { }

        //public int Register()
        //{
        //    this.readyForNextStep.Add(true);
        //    return this.nextStepUserCount++;
        //}

        //protected override bool AllReadyForNextStep()
        //{
        //    foreach (var ready in this.readyForNextStep)
        //    {
        //        if (!ready) return false;
        //    }

        //    return true;
        //}

        //public void Ready(int id)
        //{
        //    this.readyForNextStep[id] = true;
        //    this.ProcessEvents();
        //}

        //public void NotReady(int id)
        //{
        //    this.readyForNextStep[id] = false;
        //}

        private Queue<Action> work = new Queue<Action>();
        private string eventBusName;

        protected void ClearEvents()
        {
            this.work.Clear();
        }

        public void Clear()
        {
            this.ClearEvents();
            //for (int i = 0; i < this.readyForNextStep.Count; i++)
            //{
            //    this.readyForNextStep[i] = true;
            //}
        }

        public void WaitForEventCompletion()
        {
            while (this.work.Count > 0) Threading.Sleep(50);
        }

        private void Add(Action toDo)
        {
            lock (this.work)
            {
                this.work.Enqueue(toDo);
            }
        }

        #region Event handlers

        public override void HandleTournamentStarted(Scorings scoring, int maxTimePerBoard, int maxTimePerCard, string tournamentName)
        {
            if (this.OnTournamentStarted != null)
            {
                this.Add(() =>
                {
                    this.OnTournamentStarted.Invoke(scoring, maxTimePerBoard, maxTimePerCard, tournamentName);
                });
            }
        }

        public override void HandleRoundStarted(SeatCollection<string> participantNames, DirectionDictionary<string> conventionCards)
        {
            if (this.OnRoundStarted != null)
            {
                this.Add(() =>
                {
                    this.OnRoundStarted.Invoke(participantNames, conventionCards);
                });
            }
        }

        public override void HandleCardPosition(Seats seat, Suits suit, Ranks rank)
        {
            if (this.OnCardPosition != null)
            {
                this.Add(() =>
                {
                    this.OnCardPosition(seat, suit, rank);
                });
            }
        }

        public override void HandleBoardStarted(int boardNumber, Seats dealer, Vulnerable vulnerabilty)
        {

            this.Add(() =>
            {
                if (this.OnBoardStarted != null)
                {
                    this.OnBoardStarted(boardNumber, dealer, vulnerabilty);
                }
            });
        }

        public override void HandleBidNeeded(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble)
        {
            if (this.OnBidNeeded != null)
            {
                this.Add(() =>
                {
                    this.OnBidNeeded(whoseTurn, lastRegularBid, allowDouble, allowRedouble);
                });
            }
        }

        public override void HandleBidDone(Seats source, Bid bid)
        {
            this.Add(() =>
            {
                if (this.OnBidDone != null)
                {
                    this.OnBidDone(source, bid);
                }
            });
        }

        public override void HandleAuctionFinished(Seats declarer, Contract finalContract)
        {
            this.Add(() =>
            {
                if (this.OnAuctionFinished != null)
                {
                    this.OnAuctionFinished(declarer, finalContract);
                }
            });
        }

        public override void HandleCardNeeded(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
        {
            this.Add(() =>
            {
                if (this.OnCardNeeded != null)
                {
                    this.OnCardNeeded(controller, whoseTurn, leadSuit, trump, trumpAllowed, leadSuitLength, trick);
                }
            });
        }

        public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
        {
            //Log.Trace("BridgeEventBus({0}).HandleCardPlayed {1} plays {3}{2}", this.eventBusName, source, suit.ToXML(), rank.ToXML());
            this.Add(() =>
            {
                if (this.OnCardPlayed != null)
                {
                    this.OnCardPlayed(source, suit, rank);
                }
            });
        }

        public override void HandleTrickFinished(Seats trickWinner, int tricksForDeclarer, int tricksForDefense)
        {
            this.Add(() =>
            {
                if (this.OnTrickFinished != null)
                {
                    this.OnTrickFinished(trickWinner, tricksForDeclarer, tricksForDefense);
                }
            });
        }

        public override void HandlePlayFinished(BoardResultRecorder currentResult)
        {
            this.Add(() =>
            {
                if (this.OnPlayFinished != null)
                {
                    this.OnPlayFinished(currentResult);
                }
            });
        }

        public override void HandleReadyForNextStep(Seats source, NextSteps readyForStep)
        {
            this.Add(() =>
            {
                if (this.OnReadyForNextStep != null)
                {
                    this.OnReadyForNextStep(source, readyForStep);
                }
            });
        }

        public override void HandleReadyForBoardScore(int resultCount, Board2 currentBoard)
        {
            this.Add(() =>
            {
                if (this.OnOriginalDistributionRestoreFinished != null)
                {
                    this.OnOriginalDistributionRestoreFinished(resultCount, currentBoard);
                }
            });
        }

        public override void HandleTimeUsed(TimeSpan boardByNS, TimeSpan totalByNS, TimeSpan boardByEW, TimeSpan totalByEW)
        {
            this.Add(() =>
            {
                if (this.OnTimeUsed != null)
                {
                    this.OnTimeUsed(boardByNS, totalByNS, boardByEW, totalByEW);
                }
            });
        }

        public override void HandleTournamentStopped()
        {
            this.Add(() =>
            {
                if (this.OnTournamentStopped != null)
                {
                    this.OnTournamentStopped();
                }
            });
        }

        public override void HandleDummiesCardPosition(Suits suit, Ranks rank)
        {
            this.Add(() =>
            {
                if (this.OnDummiesCardPosition != null)
                {
                    this.OnDummiesCardPosition(suit, rank);
                }
            });
        }

        public override void HandleCardDealingEnded()
        {
            this.Add(() =>
            {
                if (this.OnCardDealingEnded != null)
                {
                    this.OnCardDealingEnded();
                }
            });
        }

        public override void HandleNeedDummiesCards(Seats dummy)
        {
            this.Add(() =>
            {
                if (this.OnNeedDummiesCards != null)
                {
                    this.OnNeedDummiesCards(dummy);
                }
            });
        }

        public override void HandleShowDummy(Seats dummy)
        {
            this.Add(() =>
            {
                if (this.OnShowDummy != null)
                {
                    this.OnShowDummy(dummy);
                }
            });
        }

        #endregion

        #region link/unlink

        public event TournamentStartedHandler OnTournamentStarted;
        public event RoundStartedHandler OnRoundStarted;
        public event BoardStartedHandler OnBoardStarted;
        public event CardPositionHandler OnCardPosition;
        public event BidNeededHandler OnBidNeeded;
        public event BidDoneHandler OnBidDone;
        public event AuctionFinishedHandler OnAuctionFinished;
        public event CardNeededHandler OnCardNeeded;
        public event CardPlayedHandler OnCardPlayed;
        public event TrickFinishedHandler OnTrickFinished;
        public event PlayFinishedHandler2 OnPlayFinished;
        public event ReadyForNextStepHandler OnReadyForNextStep;
        public event TournamentStoppedHandler OnTournamentStopped;
        public event ReadyForBoardScoreHandler OnOriginalDistributionRestoreFinished;
        public event TimeUsedHandler OnTimeUsed;
        public event DummiesCardPositionHandler OnDummiesCardPosition;
        public event CardDealingEndedHandler OnCardDealingEnded;
        public event ShowDummyHandler OnNeedDummiesCards;
        public event ShowDummyHandler OnShowDummy;

        public virtual void Link(BridgeEventBusClient other)
        {
            //Log.Trace("BridgeEventBus.Link {0} {1}", this.eventBusName, other.Name);
            if (other == null) throw new ArgumentNullException("other");
            this.OnTournamentStarted += new TournamentStartedHandler(other.HandleTournamentStarted);
            this.OnRoundStarted += new RoundStartedHandler(other.HandleRoundStarted);
            this.OnBoardStarted += new BoardStartedHandler(other.HandleBoardStarted);
            this.OnBidNeeded += new BidNeededHandler(other.HandleBidNeeded);
            this.OnBidDone += new BidDoneHandler(other.HandleBidDone);
            this.OnAuctionFinished += new AuctionFinishedHandler(other.HandleAuctionFinished);
            this.OnCardNeeded += new CardNeededHandler(other.HandleCardNeeded);
            this.OnCardPlayed += new CardPlayedHandler(other.HandleCardPlayed);
            this.OnTrickFinished += new TrickFinishedHandler(other.HandleTrickFinished);
            this.OnPlayFinished += new PlayFinishedHandler2(other.HandlePlayFinished);
            this.OnReadyForNextStep += new ReadyForNextStepHandler(other.HandleReadyForNextStep);
            this.OnOriginalDistributionRestoreFinished += new ReadyForBoardScoreHandler(other.HandleReadyForBoardScore);
            this.OnTimeUsed += new TimeUsedHandler(other.HandleTimeUsed);
            this.OnTournamentStopped += new TournamentStoppedHandler(other.HandleTournamentStopped);
            this.OnCardPosition += new CardPositionHandler(other.HandleCardPosition);
            this.OnDummiesCardPosition += new DummiesCardPositionHandler(other.HandleDummiesCardPosition);
            this.OnCardDealingEnded += new CardDealingEndedHandler(other.HandleCardDealingEnded);
            this.OnNeedDummiesCards += new ShowDummyHandler(other.HandleNeedDummiesCards);
            this.OnShowDummy += new ShowDummyHandler(other.HandleShowDummy);
        }

        public virtual void Unlink(BridgeEventBusClient other)
        {
            //Log.Trace("BridgeEventBus.Unlink {0} {1}", this.eventBusName, other.Name);
            this.OnTournamentStarted -= new TournamentStartedHandler(other.HandleTournamentStarted);
            this.OnRoundStarted -= new RoundStartedHandler(other.HandleRoundStarted);
            this.OnBoardStarted -= new BoardStartedHandler(other.HandleBoardStarted);
            this.OnBidNeeded -= new BidNeededHandler(other.HandleBidNeeded);
            this.OnBidDone -= new BidDoneHandler(other.HandleBidDone);
            this.OnAuctionFinished -= new AuctionFinishedHandler(other.HandleAuctionFinished);
            this.OnCardNeeded -= new CardNeededHandler(other.HandleCardNeeded);
            this.OnCardPlayed -= new CardPlayedHandler(other.HandleCardPlayed);
            this.OnTrickFinished -= new TrickFinishedHandler(other.HandleTrickFinished);
            this.OnPlayFinished -= new PlayFinishedHandler2(other.HandlePlayFinished);
            this.OnReadyForNextStep -= new ReadyForNextStepHandler(other.HandleReadyForNextStep);
            this.OnOriginalDistributionRestoreFinished -= new ReadyForBoardScoreHandler(other.HandleReadyForBoardScore);
            this.OnTimeUsed -= new TimeUsedHandler(other.HandleTimeUsed);
            this.OnTournamentStopped -= new TournamentStoppedHandler(other.HandleTournamentStopped);
            this.OnCardPosition -= new CardPositionHandler(other.HandleCardPosition);
            this.OnDummiesCardPosition -= new DummiesCardPositionHandler(other.HandleDummiesCardPosition);
            this.OnCardDealingEnded -= new CardDealingEndedHandler(other.HandleCardDealingEnded);
            this.OnNeedDummiesCards -= new ShowDummyHandler(other.HandleNeedDummiesCards);
            this.OnShowDummy -= new ShowDummyHandler(other.HandleShowDummy);
        }

        #endregion
    }

    #region All bridge event delegates

    /// <summary>
    /// Handler for TournamentStarted event
    /// </summary>
    /// <param name="scoring">Scoring method for this tournament</param>
    /// <param name="maxTimePerBoard">Maximum time a robot may use for one board</param>
    /// <param name="maxTimePerCard">Maximum time a robot may use thinking about one card</param>
    /// <param name="tournamentName">Name of the tournament</param>
    public delegate void TournamentStartedHandler(Scorings Scoring, int maxTimePerBoard, int maxTimePerCard, string tournamentName);

    /// <summary>
    /// Handler for RoundStarted event
    /// </summary>
    /// <param name="participants">All contenders in the match. Certain names ('computer') have special meaning</param>
    public delegate void RoundStartedHandler(SeatCollection<string> participantNames, DirectionDictionary<string> conventionCards);

    /// <summary>
    /// Handler for BoardStarted event
    /// </summary>
    /// <param name="boardNumber">The number of the board that just started</param>
    /// <param name="dealer">The dealer for this board</param>
    /// <param name="vulnerabilty">The vulnerability for this board</param>
    public delegate void BoardStartedHandler(int boardNumber, Seats dealer, Vulnerable vulnerabilty);

    /// <summary>
    /// Handler for CardPosition event
    /// This event fires for every card that the TournamentDirector publishes
    /// </summary>
    /// <param name="source">The owner of the card</param>
    /// <param name="suit">The suit of the card</param>
    /// <param name="rank">The rank of the card</param>
    public delegate void CardPositionHandler(Seats source, Suits suit, Ranks rank);

    /// <summary>
    /// Handler for CardPosition event
    /// This event fires for every card that the TournamentDirector publishes
    /// </summary>
    /// <param name="source">The owner of the card</param>
    /// <param name="suit">The suit of the card</param>
    /// <param name="rank">The rank of the card</param>
    public delegate void DummiesCardPositionHandler(Suits suit, Ranks rank);

    /// <summary>
    /// Handler for BidNeeded event
    /// </summary>
    /// <param name="whoseTurn">The player that must make the bid</param>
    public delegate void BidNeededHandler(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble);

    /// <summary>
    /// Handler for BidDone event
    /// </summary>
    /// <param name="source">The player that made the bid</param>
    /// <param name="bid">The bid that was made</param>
    public delegate void BidDoneHandler(Seats source, Bid bid);

    /// <summary>
    /// Handler for AuctionFinished event
    /// </summary>
    /// <param name="declarer">The declarer of the contract</param>
    /// <param name="finalContract">The bid that won the auction</param>
    /// <param name="humanActivelyInvolved">Indicator for human participation during play.
    /// Although a human is involved during bidding, he will not be involved when becoming dummy.</param>
    public delegate void AuctionFinishedHandler(Seats declarer, Contract finalContract);

    /// <summary>
    /// Handler for ShowDummy event
    /// </summary>
    /// <param name="dummy">The player that has become dummy</param>
    public delegate void ShowDummyHandler(Seats dummy);

    public delegate void CardNeededHandler(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick);

    /// <summary>
    /// Handler for CardPlayed event
    /// </summary>
    /// <param name="source">The player that played the card</param>
    /// <param name="card">The card being played</param>
    public delegate void CardPlayedHandler(Seats source, Suits suit, Ranks rank);

    /// <summary>
    /// Handler for CardHint event
    /// </summary>
    /// <param name="source">The player that played the card</param>
    /// <param name="card">The card being played</param>
    public delegate void CardHintHandler(Seats source, CardPlayedHandler callback);

    /// <summary>
    /// Handler for TrickFinished event
    /// </summary>
    /// <param name="trickWinner">The player that won this trick</param>
    /// <param name="tricksForDeclarer">Number of tricks for the declarer</param>
    /// <param name="tricksForDefense">Number of tricks for the defense</param>
    public delegate void TrickFinishedHandler(Seats trickWinner, int tricksForDeclarer, int tricksForDefense);

    /// <summary>
    /// Handler for PlayFinished event
    /// </summary>
    /// <param name="finalContract">String representation of the final result (3NT+1 +430)</param>
    /// <param name="resultCount">Number of results that exist for this board</param>
    public delegate void PlayFinishedHandler2(BoardResultRecorder currentResult);

    /// <summary>
    /// Handler for ReadyForNextStep event
    /// TournamentDirector signals all participants (UI and robots) that they must answer when ready for the next step in board play
    /// </summary>
    /// <param name="source">The player/robot who signals he is ready for the next step</param>
    /// <param name="readyForStep">Confirmation of the step he is ready for</param>
    public delegate void ReadyForNextStepHandler(Seats source, NextSteps readyForStep);

    /// <summary>
    /// Handler for ReadyForBoardScore event
    /// </summary>
    /// <param name="resultCount">Number of results that exist for this board</param>
    public delegate void ReadyForBoardScoreHandler(int resultCount, Board2 currentBoard);

    /// <summary>
    /// Handler for TournamentStopped event
    /// </summary>
    public delegate void TournamentStoppedHandler();

    /// <summary>
    /// Handler for CardDealingEnded event
    /// </summary>
    public delegate void CardDealingEndedHandler();

    public delegate void StatusChangedHandler(string status);

    public delegate void TimeUsedHandler(TimeSpan boardByNS, TimeSpan totalByNS, TimeSpan boardByEW, TimeSpan totalByEW);

    /// <summary>
    /// Handler for LongTrace event
    /// </summary>
    /// <param name="trace">Trace data</param>
    public delegate void LongTraceHandler(string trace);
    #endregion

    /// <summary>
    /// Possible steps that occur during play of a board
    /// </summary>
    public enum NextSteps
    {
        /// <summary>
        /// Prepare for play of the board
        /// </summary>
        NextStartPlay,

        /// <summary>
        /// Prepare for the next trick
        /// </summary>
        NextTrick,

        /// <summary>
        /// Prepare for showing the result of this board
        /// </summary>
        NextShowScore,

        /// <summary>
        /// Prepare for the next board
        /// </summary>
        NextBoard

        /// <summary>
        /// Prepare for the same board
        /// </summary>
        , SameBoard
    }
}
