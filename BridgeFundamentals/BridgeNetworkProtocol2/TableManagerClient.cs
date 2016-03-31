#if !CHAMPIONSHIP
//#define Olympus
#endif
#if DEBUG
#define syncTrace   // uncomment to get detailed trace of events and protocol messages
#endif

using System;
using System.Collections.Generic;
using Sodes.Bridge.Base;
using System.Threading.Tasks;
using Sodes.Base;
using BridgeNetworkProtocol2;

namespace Sodes.Bridge.Networking
{
    /// <summary>
    /// Implementation of the client side of the Bridge Network Protocol
    /// as described in http://www.bluechipbridge.co.uk/protocol.htm
    /// </summary>
    public abstract class TableManagerClient : BoardResultOwner
    {
        public TableManagerProtocolState state;
        public event BridgeNetworkEventHandler OnBridgeNetworkEvent;
        private string teamNS;
        private string teamEW;
        public Seats seat;
        private string seatName;
        private Seats theDealer;
        private bool isDeclarer;
        private string team;
        private int maxTimePerBoard;
        private int maxTimePerCard;
        //private bool sendAlerts;
        private Queue<string> messages;
        private string[] tableManagerExpectedResponse;
        private object lockerProcessMessage = new object();
        private object locker = new object();
        private Card leadCard;
        private Queue<StateChange> stateChanges;
        private bool _waitForProtocolSync;
        private bool _waitForBridgeEvents;
        private bool moreBoards;

        protected TableManagerClient(BridgeEventBus bus)
            : base("TableManagerClient", bus)
        {
            this.messages = new Queue<string>();
            this.stateChanges = new Queue<StateChange>();
            this._waitForBridgeEvents = false;
            this._waitForProtocolSync = false;
            this.moreBoards = true;
            Task.Run(async () =>
            {
                await this.ProcessMessages();
            });
            Task.Run(async () =>
            {
                await this.ProcessStateChanges();
            });
        }

        protected TableManagerClient() : this(null) { }

        private async Task ProcessMessages()
        {
            const int minimumWait = 0;
            var waitForNewMessage = minimumWait;
            do
            {
                waitForNewMessage = 5;

                while (this.messages.Count >= 1 && !this.WaitForBridgeEvents)
                {
                    waitForNewMessage = minimumWait;
                    string message = string.Empty;
                    lock (this.messages)
                    {
                        message = this.messages.Dequeue();
                    }

                    this.ProcessMessage(message);
                } 

                if (waitForNewMessage > minimumWait)
                {
                    //Log.Trace("Host out of messages");
                    await Task.Delay(waitForNewMessage);
                }
            } while (this.moreBoards);
        }

        private async Task ProcessStateChanges()
        {
            const int minimumWait = 0;
            var waitForNewMessage = minimumWait;
            do
            {
                waitForNewMessage = 5;

                while (this.stateChanges.Count > 0 && !this.WaitForProtocolSync)
                {
                    waitForNewMessage = minimumWait;
                    StateChange stateChange;
                    lock (this.stateChanges) stateChange = this.stateChanges.Dequeue();
                    //if (this.state != stateChange.NewState)
                    {
                        this.state = stateChange.NewState;
#if syncTrace
                        Log.Trace(2, "Client {0} new state {1} message='{2}' expected='{3}'", this.seat, this.state, stateChange.Message, stateChange.ExpectedResponses[0]);
#endif
                    }

                    this.tableManagerExpectedResponse = stateChange.ExpectedResponses;
                    if (stateChange.Message.Length > 0)
                    {
                        await this.WriteProtocolMessageToRemoteMachine(stateChange.Message);
                    }

                    this.WaitForProtocolSync = stateChange.WaitForSync;        // e.g. must wait for 'to lead' message
                    this.WaitForBridgeEvents = stateChange.WaitForStateChanges;
                }

                if (waitForNewMessage > minimumWait)
                {
                    //Log.Trace("Host out of messages");
                    await Task.Delay(waitForNewMessage);
                }
            } while (this.moreBoards);
        }

        private bool WaitForBridgeEvents
        {
            get
            {
                lock (this.locker)
                {
                    return this._waitForBridgeEvents;
                }
            }
            set
            {
                if (value != this._waitForBridgeEvents)
                {
                    lock (this.locker)
                    {
                        this._waitForBridgeEvents = value;
                    }
#if syncTrace
                    Log.Trace(2, "Client {0} {1} processing messages", seat, value ? "pauses" : "resumes");
#endif
                }
            }
        }

        private bool WaitForProtocolSync
        {
            get
            {
                lock (this.locker)
                {
                    return this._waitForProtocolSync;
                }
            }
            set
            {
                if (value != this._waitForProtocolSync)
                {
                    lock (this.locker)
                    {
                        this._waitForProtocolSync = value;
                    }
#if syncTrace
                    Log.Trace(2, "Client {0} {1} state changes", seat, value ? "pauses" : "resumes");
#endif
                }
            }
        }

        public void Connect(Seats _seat, int _maxTimePerBoard, int _maxTimePerCard, string teamName, int botCount, bool _sendAlerts)
        {
            this.seat = _seat;
            this.seatName = seat.ToString();		// Seat.ToXML(seat);
            this.team = teamName;
            this.maxTimePerBoard = _maxTimePerBoard;
            this.maxTimePerCard = _maxTimePerCard;
            //this.sendAlerts = _sendAlerts;
            this.WaitForProtocolSync = false;
            this.WaitForBridgeEvents = false;

            this.ChangeState(TableManagerProtocolState.WaitForSeated
                , false, false
                , new string[] { string.Format("{0} (\"{1}\") seated", seatName, teamName)
                                ,string.Format("{0} {1} seated", seatName, teamName)
                                }
                , "Connecting \"{0}\" as {1} using protocol version 18", this.team, this.seat);
        }

        protected abstract Task WriteProtocolMessageToRemoteMachine(string message);

        private void ProcessMessage(string message)
        {
#if syncTrace
            Log.Trace(2, "Client {1} processing '{0}'", message, seat);
#endif
            if (message.StartsWith("NS:"))		// something new from Bridge Moniteur: session ends with 
            {
                this.EventBus.HandleTournamentStopped();
                this.state = TableManagerProtocolState.Finished;
                if (this.OnBridgeNetworkEvent != null) this.OnBridgeNetworkEvent(this, BridgeNetworkEvents.SessionEnd, new BridgeNetworkEventData());
                return;
            }

            if (message == "End of session")
            {
                this.EventBus.HandleTournamentStopped();
                this.ChangeState(TableManagerProtocolState.WaitForDisconnect, false, false, new string[] { "Disconnect" }, "");
                return;
            }

            if (Expected(message))
            {
                try
                {
                    switch (this.state)
                    {
                        case TableManagerProtocolState.WaitForSeated:
                            if (this.OnBridgeNetworkEvent != null) this.OnBridgeNetworkEvent(this, BridgeNetworkEvents.Seated, new BridgeNetworkEventData());
                            this.ChangeState(TableManagerProtocolState.WaitForTeams, false, false, new string[] { "Teams" }, "{0} ready for teams", this.seat);
                            break;

                        case TableManagerProtocolState.WaitForTeams:
                            this.teamNS = message.Substring(message.IndexOf("N/S : \"") + 7);
                            this.teamNS = teamNS.Substring(0, teamNS.IndexOf("\""));
                            this.teamEW = message.Substring(message.IndexOf("E/W : \"") + 7);
                            this.teamEW = teamEW.Substring(0, teamEW.IndexOf("\""));

                            if (this.OnBridgeNetworkEvent != null) this.OnBridgeNetworkEvent(this, BridgeNetworkEvents.Teams, new BridgeNetworkTeamsEventData(teamNS, teamEW));

                            this.ChangeState(TableManagerProtocolState.WaitForStartOfBoard, false, false, new string[] { "Start of board", "End of session" }, "{0} ready to start", this.seat);
                            break;

                        case TableManagerProtocolState.WaitForStartOfBoard:
                            if (message.StartsWith("Teams"))
                            {		// bug in BridgeMoniteur when tournament is restarted
                            }
                            else if (message.StartsWith("Timing"))
                            {
                                // Timing - N/S : this board [minutes:seconds], total [hours:minutes:seconds]. E/W : this board [minutes:seconds], total [hours:minutes:seconds]".
                                // Bridge Moniteur does not send the '.' at the end of the message
                                // Timing - N/S : this board  01:36,  total  0:01:36.  E/W : this board  01:34,  total  0:01:34
                                if (!message.EndsWith(".")) message += ".";
                                string[] timing = message.Split('.');
                                string[] parts = timing[0].Split(',');
                                string boardNS = "00:" + parts[0].Substring(parts[0].IndexOf("board") + 6).Trim();
                                string totalNS = parts[1].Substring(parts[1].IndexOf("total") + 6).Trim();
                                parts = timing[1].Split(',');
                                string boardEW = "00:" + parts[0].Substring(parts[0].IndexOf("board") + 6).Trim();
                                string totalEW = parts[1].Substring(parts[1].IndexOf("total") + 6).Trim();

                                TimeSpan _boardNS = ParseTimeUsed(boardNS);
                                TimeSpan _totalNS = ParseTimeUsed(totalNS);
                                TimeSpan _boardEW = ParseTimeUsed(boardEW);
                                TimeSpan _totalEW = ParseTimeUsed(totalEW);
                                this.EventBus.HandleTimeUsed(_boardNS, _totalNS, _boardEW, _totalEW);
                            }
                            else
                            {
                                this.ChangeState(TableManagerProtocolState.WaitForBoardInfo, false, false, new string[] { "Board number" }, "{0} ready for deal", this.seat);
                            }
                            break;

                        case TableManagerProtocolState.WaitForBoardInfo:
                            // "Board number 1. Dealer North. Neither vulnerable."
                            string[] dealInfoParts = message.Split('.');
                            int boardNumber = Convert.ToInt32(dealInfoParts[0].Substring(13));
                            this.theDealer = SeatsExtensions.FromXML(dealInfoParts[1].Substring(8));
                            Vulnerable vulnerability = Vulnerable.Neither;
                            switch (dealInfoParts[2].Substring(1))
                            {
                                case "Both vulnerable":
                                    vulnerability = Vulnerable.Both; break;
                                case "N/S vulnerable":
                                    vulnerability = Vulnerable.NS; break;
                                case "E/W vulnerable":
                                    vulnerability = Vulnerable.EW; break;
                            }

                            var board = new Board2(this.theDealer, vulnerability, new Distribution());
                            this.CurrentResult = new TMBoardResult(this, board, new SeatCollection<string>(new string[] { this.teamNS, this.teamEW, this.teamNS, this.teamEW }));

                            this.EventBus.HandleBoardStarted(boardNumber, this.theDealer, vulnerability);
                            this.ChangeState(TableManagerProtocolState.WaitForMyCards, false, false, new string[] { this.seat + "'s cards : " }, "{0} ready for cards", this.seat);
                            break;

                        case TableManagerProtocolState.WaitForMyCards:
                            // "North's cards : S J 8 5.H A K T 8.D 7 6.C A K T 3."
                            // "North's cards : S J 8 5.H A K T 8.D.C A K T 7 6 3."
                            // "North's cards : S -.H A K T 8 4 3 2.D.C A K T 7 6 3."
                            string cardInfo = message.Substring(2 + message.IndexOf(":"));
                            string[] suitInfo = cardInfo.Split('.');
                            for (int s1 = 0; s1 < 4; s1++)
                            {
                                suitInfo[s1] = suitInfo[s1].Trim();
                                Suits s = SuitHelper.FromXML(suitInfo[s1].Substring(0, 1));
                                if (suitInfo[s1].Length > 2)
                                {
                                    string cardsInSuit = suitInfo[s1].Substring(2) + " ";
                                    if (cardsInSuit.Substring(0, 1) != "-")
                                    {
                                        while (cardsInSuit.Length > 1)
                                        {
                                            Ranks rank = Rank.From(cardsInSuit.Substring(0, 1));
                                            this.EventBus.HandleCardPosition(this.seat, s, rank);
                                            cardsInSuit = cardsInSuit.Substring(2);
                                        }
                                    }
                                }
                            }

                            // TM is now expecting a response: either a bid or a 'ready for bid'
                            this.EventBus.HandleCardDealingEnded();

                            break;

                        case TableManagerProtocolState.WaitForOtherBid:
                            this.WaitForBridgeEvents = true;
                            ProtocolHelper.HandleProtocolBid(message, this.EventBus);
                            break;

                        case TableManagerProtocolState.WaitForDummiesCards:
                            //Log.Trace("Client {1} processing dummies cards", message, seat);
                            string dummiesCards = message.Substring(2 + message.IndexOf(":"));
                            string[] suitInfo2 = dummiesCards.Split('.');
                            for (Suits s = Suits.Spades; s >= Suits.Clubs; s--)
                            {
                                int suit = 3 - (int)s;
                                suitInfo2[suit] = suitInfo2[suit].Trim();
                                if (suitInfo2[suit].Length > 2)
                                {
                                    string cardsInSuit = suitInfo2[suit].Substring(2) + " ";
                                    if (cardsInSuit.Substring(0, 1) != "-")
                                    {
                                        while (cardsInSuit.Length > 1)
                                        {
                                            Ranks rank = Rank.From(cardsInSuit.Substring(0, 1));
                                            this.EventBus.HandleCardPosition(this.CurrentResult.Play.Dummy, s, rank);
                                            cardsInSuit = cardsInSuit.Substring(2);
                                        }
                                    }
                                }
                            }

                            this.WaitForProtocolSync = false;
                            this.EventBus.HandleShowDummy(this.CurrentResult.Play.Dummy);
                            break;

                        case TableManagerProtocolState.WaitForLead:
                            this.WaitForProtocolSync = false;
                            break;

                        case TableManagerProtocolState.WaitForCardPlay:
                            if (message.Contains("to lead"))
                            {
                                /// This indicates a timing issue: TM sent a '... to lead' message before TD sent its HandleTrickFinished event
                                /// Wait until I receveive the HandleTrickFinished event
                                Log.Trace(1, "TableManagerClient.ProcessMessage {0}: received 'to lead' before HandleTrickFinished", this.seat);
                                //Debugger.Break();
                            }
                            else
                            {
                                string[] cardPlay = message.Split(' ');
                                Seats player = SeatsExtensions.FromXML(cardPlay[0]);
                                Card card = new Card(SuitHelper.FromXML(cardPlay[2].Substring(1, 1)), Rank.From(cardPlay[2].Substring(0, 1)));
                                if (player != this.CurrentResult.Play.Dummy) this.EventBus.HandleCardPosition(player, card.Suit, card.Rank);
                                this.WaitForBridgeEvents = true;
                                this.EventBus.HandleCardPlayed(player, card.Suit, card.Rank);
                            }

                            break;

                        case TableManagerProtocolState.WaitForDisconnect:
                            this.state = TableManagerProtocolState.Finished;
                            this.EventBus.HandleTournamentStopped();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Error while processing message '{0}' in state {1}", message, state), ex);
                }
            }
            else
            {		// unexpected message

            }
        }

        private static TimeSpan ParseTimeUsed(string boardTime)
        {
            TimeSpan result;
            try
            {
                result = TimeSpan.Parse(boardTime);
                // bug in TableManager: boards played over midnight get 24h complement of spent time
                if (result.TotalHours >= 22) result = new TimeSpan(0, 2, 0);
            }
            catch (ArgumentNullException)
            {
                result = new TimeSpan(0, 1, 50);
            }
            catch (FormatException)
            {
                result = new TimeSpan(0, 1, 50);
            }
            catch (OverflowException)
            {
                result = new TimeSpan(0, 1, 50);
            }

            return result;
        }

        private void ChangeState(TableManagerProtocolState newState, bool waitForSync, bool waitForMoreStateChanges, string[] expectedAnswers, string message, params object[] args)
        {
            message = string.Format(message, args);
            var stateChange = new StateChange() { Message = message, NewState = newState, ExpectedResponses = expectedAnswers, WaitForSync = waitForSync, WaitForStateChanges = waitForMoreStateChanges };
            lock (this.stateChanges) this.stateChanges.Enqueue(stateChange);
#if syncTrace
            //Log.Trace("Client {0} queued state change {1} ({2} states on the q)", this.seat, newState, this.stateChanges.Count);
#endif
        }

        private bool Expected(string message)
        {
            string loweredMessage = message.ToLowerInvariant();
            if (loweredMessage.StartsWith("unexpected "))
            {
                throw new InvalidOperationException(string.Format("Table Manager tells me I screwed up: '{0}'", message));
            }

            if (loweredMessage.StartsWith("disconnect"))
            {
                this.state = TableManagerProtocolState.WaitForDisconnect;
                return true;
            }

            for (int a = 0; a < this.tableManagerExpectedResponse.Length; a++)
            {
                if (loweredMessage.StartsWith(this.tableManagerExpectedResponse[a].ToLowerInvariant()))
                {
                    return true;
                }
            }

            if (message.StartsWith("Teams")) return true;		// bug in BridgeMoniteur
            Log.Trace(0, "Unexpected response by {3}: '{0}' in state {2}; expected '{1}'", message, this.tableManagerExpectedResponse[0], this.state, this.seat);
#if DEBUG
            //System.Diagnostics.Debugger.Break();
#endif
            throw new InvalidOperationException(string.Format("Unexpected response by {2} '{0}'; expected '{1}'", message, this.tableManagerExpectedResponse[0], this.seat));
        }

        protected void ProcessIncomingMessage(string message)
        {
#if syncTrace
            Log.Trace(3, "TableManagerClient.{0}.ProcessIncomingMessage queues '{1}'", this.seat, message);
#endif
            lock (this.messages) this.messages.Enqueue(message);
        }

        #region Bridge Event Handlers

        public override void HandleTournamentStopped()
        {
            base.HandleTournamentStopped();
            this.moreBoards = false;
            if (this.OnBridgeNetworkEvent != null) this.OnBridgeNetworkEvent(this, BridgeNetworkEvents.SessionEnd, null);
        }

        #endregion

        protected override BoardResultRecorder NewBoardResult(int boardNumber)
        {
            return new TMBoardResult(this, null, new SeatCollection<string>(new string[] { "","","",""}));
        }

        private struct StateChange
        {
            public string Message { get; set; }
            public TableManagerProtocolState NewState { get; set; }
            public string[] ExpectedResponses { get; set; }
            public bool WaitForSync { get; set; }
            public bool WaitForStateChanges { get; set; }
        }

        private class TMBoardResult : BoardResultEventPublisher
        {
            public TMBoardResult(TableManagerClient _owner, Board2 board, SeatCollection<string> newParticipants)
                : base("TMBoardResult." + _owner.seat, board, newParticipants, _owner.EventBus)
            {
                this.tmc = _owner;
            }

            private TableManagerClient tmc;

            public override void HandleBidNeeded(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble)
            {
#if syncTrace
                Log.Trace(2, "{0}.HandleBidNeeded: from {1}", this.Owner, whoseTurn);
#endif
                if (this.tmc.seat != whoseTurn)
                {
                    this.tmc.ChangeState(TableManagerProtocolState.WaitForOtherBid
                        , false, false
                        , new string[] { whoseTurn.ToString() }
                        , "{0} ready for {1}'s bid", this.tmc.seat, whoseTurn);
                }
            }

            public override async void HandleBidDone(Seats source, Bid bid)
            {
#if syncTrace
                Log.Trace(2, "{0}.HandleBidDone: {1} bids {2}", this.Owner, source, bid);
#endif
                base.HandleBidDone(source, bid);
                this.tmc.WaitForBridgeEvents = this.Auction.Ended;
                if (source == this.tmc.seat)
                {
                    await this.tmc.WriteProtocolMessageToRemoteMachine(ProtocolHelper.Translate(bid, source));
                }
            }

            public override void HandleNeedDummiesCards(Seats dummy)
            {
#if syncTrace
                //Log.Trace(2, "{0}.HandleNeedDummiesCards", this.Owner);
#endif
                if (this.tmc.seat != dummy)
                {
                    this.tmc.ChangeState(TableManagerProtocolState.WaitForDummiesCards, false, false, new string[] { "Dummy's cards :" }, "{0} ready for dummy", this.tmc.seat);
                }
                else
                {
                    this.EventBus.HandleShowDummy(dummy);
                }
            }

            public override void HandleCardNeeded(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
            {
#if syncTrace
                Log.Trace(2, "{0}.HandleCardNeeded", this.Owner);
#endif
                if (whoseTurn != this.Play.whoseTurn) throw new InvalidOperationException("whoseTurn");
                if (controller == this.tmc.seat)
                {
                    if (this.Play.man == 1)
                    {
                        this.tmc.ChangeState(TableManagerProtocolState.WaitForLead, true, false, new string[] { string.Format("{0} to lead", this.Play.whoseTurn == this.Play.Dummy ? "Dummy" : this.Play.whoseTurn.ToXMLFull()) }, "");
                    }
                }
                else
                {
#if syncTrace
                    //Log.Trace("{0}.HandleCardNeeded from {1}", this.Owner.seat, whoseTurn);
#endif
                    this.tmc.ChangeState(TableManagerProtocolState.WaitForCardPlay
                        , false, false
                        , new string[] { "" }
                        , "{0} ready for {1}'s card to trick {2}", this.tmc.seat, (this.tmc.seat == this.Play.Dummy && this.tmc.seat == whoseTurn ? "dummy" : whoseTurn.ToString()), trick);
                }
            }

            public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
            {
#if syncTrace
                Log.Trace(2, "{0}.HandleCardPlayed: {1} plays {3} of {2}", this.Owner, source, suit, rank);
#endif
                //this.tmc.WaitForBridgeEvents = true;
                var manForCurrentCard = this.Play.man;
                base.HandleCardPlayed(source, suit, rank);
#if syncTrace
                Log.Trace(2, "{0}.HandleCardPlayed: next card by {1}", this.Owner, this.Play.whoseTurn);
#endif
                if ((source == this.tmc.seat && this.tmc.seat != this.Play.Dummy) || (source == this.Play.Dummy && this.tmc.seat == this.Play.Dummy.Partner()))
                {
                    var message = string.Format("{0} plays {2}{1}",
                            // TM bug: TM does not recognize the phrase 'Dummy plays 8C', eventhough the protocol defines it
                            //whoseTurn != this.seat ? "Dummy" : whoseTurn.ToString(),
                            source,
                            SuitHelper.ToXML(suit),
                            rank.ToXML());

                    if (manForCurrentCard == 1)
                    {
                        this.tmc.ChangeState(TableManagerProtocolState.WaitForLead, false, false, new string[] { "" }, message);
                    }
                    else
                    {
                        this.tmc.ChangeState(this.tmc.state, false, this.Play.PlayEnded || (this.Play.man == 1 && (this.Play.whoseTurn == this.tmc.seat || (this.Play.whoseTurn == this.Play.Dummy && this.tmc.seat == this.Play.Contract.Declarer))), this.tmc.tableManagerExpectedResponse, message);
                    }
                }

                //this.tmc.WaitForBridgeEvents = (this.Play.PlayEnded || this.Play.whoseTurn == this.tmc.seat);
            }

            public override void HandlePlayFinished(BoardResultRecorder currentResult)
            {
#if syncTrace
                //Log.Trace("{0}.HandlePlayFinished", this.Owner);
#endif
                base.HandlePlayFinished(currentResult);
                this.tmc.ChangeState(TableManagerProtocolState.WaitForStartOfBoard, false, false, new string[] { "Start of board", "End of session", "Timing", "NS" }, "");
            }
        }
    }

    public enum BridgeNetworkEvents
    {
        Seated
        ,
        Teams
        ,
        Error
            ,
        SessionEnd
    }

    public class BridgeNetworkEventData
    {
    }

    public class BridgeNetworkTeamsEventData : BridgeNetworkEventData
    {
        public string NS;
        public string EW;

        public BridgeNetworkTeamsEventData(string _ns, string _ew)
        {
            this.NS = _ns;
            this.EW = _ew;
        }
    }

    public delegate void BridgeNetworkEventHandler(object sender, BridgeNetworkEvents e, BridgeNetworkEventData data);
}
