#if DEBUG
#define syncTrace   // uncomment to get detailed trace of events and protocol messages
#endif

using System;
using Sodes.Bridge.Base;
using System.Threading.Tasks;
using BridgeNetworkProtocol2;
using System.Collections.Concurrent;
using System.IO;
using Sodes.Base;

namespace Sodes.Bridge.Networking
{
	public enum HostEvents { Seated, ReadyForTeams, ReadyToStart, ReadyForDeal, ReadyForCards, ReadyForDummiesCards }
    public delegate void HandleHostEvent(TableManagerHost sender, HostEvents hostEvent, Seats seat, string message);

    /// <summary>
    /// Implementation of the server side of the Bridge Network Protocol
    /// as described in http://www.bluechipbridge.co.uk/protocol.htm
    /// </summary>
    public abstract class TableManagerHost : BridgeEventBusClient
    {
		internal class ClientData
		{
			public TableManagerProtocolState state;
			public string teamName;
			public string hand;
            public ConcurrentQueue<ClientMessage> messages;
            public long communicationLag;
            private bool _pause;
            public bool Pause
            {
                get { return _pause; }
                set
                {
                    if (value != _pause)
                    {
                        _pause = value;
#if syncTrace
                        Log.Trace(3, "Host {1} {0}", hand, _pause ? "pauses" : "resumes");
#endif
                    }
                }
            }
		}

        public class ClientMessage
		{
			public Seats Seat { get; set; }
			public string Message { get; set; }
			public ClientMessage(Seats s, string m)
			{
				this.Seat = s;
				this.Message = m;
			}
		}

		public event HandleHostEvent OnHostEvent;

		internal SeatCollection<ClientData> clients;
        private string lastRelevantMessage;
        private bool moreBoards;
        private bool allReadyForStartOfBoard;
        private bool allReadyForDeal;
        private TournamentController c;
        private BoardResultRecorder CurrentResult;
        private DirectionDictionary<TimeSpan> totalTime;
        private DirectionDictionary<TimeSpan> boardTime;
        private System.Diagnostics.Stopwatch lagTimer;

        protected TableManagerHost(BridgeEventBus bus) : base(bus, "TableManagerHost")
		{
			this.clients = new SeatCollection<ClientData>();
			for (Seats seat = Seats.North; seat <= Seats.West; seat++)
			{
				this.clients[seat] = new ClientData();
				this.clients[seat].state = TableManagerProtocolState.Initial;
                this.clients[seat].Pause = false;
                this.clients[seat].messages = new ConcurrentQueue<ClientMessage>();
            }

            this.moreBoards = true;
            this.lagTimer = new System.Diagnostics.Stopwatch();
            Task.Run(async () =>
            {
                await this.ProcessMessages();
            });
        }

        public void HostTournament(string pbnTournament)
        {
            var t = TournamentLoader.LoadAsync(File.OpenRead(pbnTournament)).Result;
            this.c = new TMController(this, t, new ParticipantInfo() { ConventionCardNS = this.clients[Seats.North].teamName, ConventionCardWE = this.clients[Seats.East].teamName, MaxThinkTime = 120, UserId = Guid.NewGuid(), PlayerNames = new Participant(this.clients[Seats.North].teamName, this.clients[Seats.East].teamName, this.clients[Seats.North].teamName, this.clients[Seats.East].teamName) }, this.EventBus);
            this.allReadyForStartOfBoard = false;
            this.c.StartTournament();
            this.totalTime = new DirectionDictionary<TimeSpan>(new TimeSpan(), new TimeSpan());
        }

        protected void ProcessIncomingMessage(string message, Seats seat)
		{
			lock (this.clients[seat].messages)
			{
				this.clients[seat].messages.Enqueue(new ClientMessage(seat, message));
#if syncTrace
                Log.Trace(3, "Host queued {0}'s '{1}' ({2} messages on q)", seat, message, this.clients[seat].messages.Count);
#endif
            }
		}

		private async Task ProcessMessages()
		{
            const int minimumWait = 10;
            var waitForNewMessage = minimumWait;
			ClientMessage m = null;
			do
			{
                waitForNewMessage = 20;
                for (Seats seat = Seats.North; seat <= Seats.West; seat++)
                {
                    if (!this.clients[seat].Pause && !this.clients[seat].messages.IsEmpty)
                    {
                        lock (this.clients[seat].messages)
                        {
                            this.clients[seat].messages.TryDequeue(out m);
                        }

                        if (m != null)
                        {
#if syncTrace
                            Log.Trace(3, "Host dequeued {0}'s '{1}'", m.Seat, m.Message);
#endif
                            waitForNewMessage = minimumWait;
                            lock (this.clients)
                            {       // ensure exclusive access to ProcessMessage
                                this.ProcessMessage(m.Message, m.Seat);
                            }
                        }
                    }
                }

                if (waitForNewMessage > minimumWait)
                {
#if syncTrace
                    //Log.Trace("Host out of messages");
#endif
                    await Task.Delay(waitForNewMessage);
                }
            } while (this.moreBoards);
		}

#if syncTrace
        private void DumpQueue()
        {
            Log.Trace(1, "Host remaining messages on queue:");
            for (Seats seat = Seats.North; seat <= Seats.West; seat++)
            {
                var more = true;
                while (more)
                {
                    ClientMessage m = null;
                    this.clients[seat].messages.TryDequeue(out m);
                    if (m == null)
                    {
                        more = false;
                    }
                    else
                    { 
                        Log.Trace(1, "Host queue item {0} '{1}'", m.Seat, m.Message);
                    }
                }
            }
        }
#endif

		private void ProcessMessage(string message, Seats seat)
		{
#if syncTrace
			Log.Trace(2, "Host processing '{0}'", message);
#endif
            switch (this.clients[seat].state)
			{
				case TableManagerProtocolState.Initial:
					if (message.ToLowerInvariant().Contains("connecting") && message.ToLowerInvariant().Contains("using protocol version"))
					{
						int p = message.IndexOf("\"");
						this.clients[seat].teamName = message.Substring(p + 1, message.IndexOf("\"", p + 1) - (p + 1));
						this.clients[seat].hand = seat.ToString();		//.Substring(0, 1)
						this.WriteData(seat, "{1} (\"{0}\") seated", this.clients[seat].teamName, this.clients[seat].hand);
						this.clients[seat].state = TableManagerProtocolState.WaitForSeated;
					}
					else
					{
						this.Refuse(seat, "Expected 'Connecting ....'");
					}
					break;

				case TableManagerProtocolState.WaitForSeated:
					ChangeState(message, this.clients[seat].hand + " ready for teams", TableManagerProtocolState.WaitForTeams, seat);
					break;

				case TableManagerProtocolState.WaitForTeams:
                    this.UpdateCommunicationLag(seat, this.lagTimer.ElapsedTicks);
					ChangeState(message, this.clients[seat].hand + " ready to start", TableManagerProtocolState.WaitForStartOfBoard, seat);
					break;

				case TableManagerProtocolState.WaitForStartOfBoard:
                    this.UpdateCommunicationLag(seat, this.lagTimer.ElapsedTicks);
                    ChangeState(message, this.clients[seat].hand + " ready for deal", TableManagerProtocolState.WaitForBoardInfo, seat);
					break;

				case TableManagerProtocolState.WaitForBoardInfo:
                    this.UpdateCommunicationLag(seat, this.lagTimer.ElapsedTicks);
                    ChangeState(message, this.clients[seat].hand + " ready for cards", TableManagerProtocolState.WaitForMyCards, seat);
					this.OnHostEvent(this, HostEvents.ReadyForCards, seat, string.Empty);
					break;

				case TableManagerProtocolState.WaitForMyCards:
                    lock (this.clients) this.clients[seat].Pause = true;
                    if (seat == this.CurrentResult.Auction.WhoseTurn)
                    {
                        if (message.Contains(" ready for "))
                        {
#if syncTrace
                            Log.Trace(0, "Host expected '... bids ..' from {0}", seat);
                            this.DumpQueue();
#endif
                            throw new InvalidOperationException();
                        }

                        this.lastRelevantMessage = message;
#if syncTrace
                        //Log.Trace("Host lastRelevantMessage={0}", message);
#endif
                        ChangeState(message, this.clients[seat].hand + " ", TableManagerProtocolState.WaitForOtherBid, seat);
                    }
                    else
                    {
                        ChangeState(message, string.Format("{0} ready for {1}'s bid", this.clients[seat].hand, this.CurrentResult.Auction.WhoseTurn), TableManagerProtocolState.WaitForOtherBid, seat);
                    }
                    break;

                case TableManagerProtocolState.WaitForCardPlay:
                    lock (this.clients) this.clients[seat].Pause = true;
                    // ready for dummy's card mag ook ready for xx's card
                    if (this.CurrentResult.Play.whoseTurn == this.CurrentResult.Play.Dummy && seat == this.CurrentResult.Play.Dummy)
					{
						ChangeState(message, string.Format("{0} ready for dummy's card to trick {2}", this.clients[seat].hand, message.Contains("dummy") ? "dummy" : this.CurrentResult.Play.whoseTurn.ToString(), this.CurrentResult.Play.currentTrick), TableManagerProtocolState.WaitForOtherCardPlay, seat);
					}
					else
					{
						ChangeState(message, string.Format("{0} ready for {1}'s card to trick {2}", this.clients[seat].hand, this.CurrentResult.Play.whoseTurn.ToString(), this.CurrentResult.Play.currentTrick), TableManagerProtocolState.WaitForOtherCardPlay, seat);
					}
					break;

				case TableManagerProtocolState.WaitForOwnCardPlay:
                    lock (this.clients) this.clients[seat].Pause = true;
                    this.lastRelevantMessage = message;
#if syncTrace
                    //Log.Trace("Host lastRelevantMessage={0}", message);
#endif
                    ChangeState(message, string.Format("{0} plays ", this.CurrentResult.Play.whoseTurn), TableManagerProtocolState.WaitForOtherCardPlay, seat);
                    break;

				case TableManagerProtocolState.WaitForDummiesCardPlay:
					ChangeState(message, string.Format("{0} plays ", this.CurrentResult.Play.whoseTurn), TableManagerProtocolState.WaitForOtherCardPlay, seat);
					break;

				case TableManagerProtocolState.WaitForDummiesCards:
					ChangeState(message, string.Format("{0} ready for dummy", this.clients[seat].hand), TableManagerProtocolState.GiveDummiesCards, seat);
					break;

				default:
#if syncTrace
                    Log.Trace(0, "Host unexpected '{0}' from {1} in state {2}", message, seat, this.clients[seat].state);
                    this.DumpQueue();
#endif
                    this.Refuse(seat, "Unexpected '{0}' in state {1}", message, this.clients[seat].state);
                    throw new InvalidOperationException(string.Format("Unexpected '{0}' in state {1}", message, this.clients[seat].state));
			}
		}

		private void ChangeState(string message, string expected, TableManagerProtocolState newState, Seats seat)
		{
			if (message.ToLowerInvariant().StartsWith(expected.ToLowerInvariant()))
			{
				this.clients[seat].state = newState;
                var allReady = true;
                for (Seats s = Seats.North; s <= Seats.West; s++) if (this.clients[s].state != newState) allReady = false;
                if (allReady)
                {
#if syncTrace
                    Log.Trace(2, "Host ChangeState {0}", newState);
#endif
                    switch (newState)
                    {
                        case TableManagerProtocolState.Initial:
                            break;
                        case TableManagerProtocolState.WaitForSeated:
                            break;
                        case TableManagerProtocolState.WaitForTeams:
                            this.BroadCast("Teams : N/S : \"" + this.clients[Seats.North].teamName + "\". E/W : \"" + this.clients[Seats.East].teamName + "\"");
                            this.OnHostEvent(this, HostEvents.ReadyForTeams, Seats.North, "");
                            break;
                        case TableManagerProtocolState.WaitForStartOfBoard:
                            this.c.StartNextBoard().Wait();
                            break;
                        case TableManagerProtocolState.WaitForBoardInfo:
                            this.BroadCast("Board number {0}. Dealer {1}. {2} vulnerable.", this.c.currentBoard.BoardNumber, this.c.currentBoard.Dealer.ToXMLFull(), ProtocolHelper.Translate(this.c.currentBoard.Vulnerable));
                            break;
                        case TableManagerProtocolState.WaitForMyCards:
                            this.WriteData(Seats.North, ProtocolHelper.Translate(Seats.North, this.c.currentBoard.Distribution));
                            this.WriteData(Seats.East, ProtocolHelper.Translate(Seats.East, this.c.currentBoard.Distribution));
                            this.WriteData(Seats.South, ProtocolHelper.Translate(Seats.South, this.c.currentBoard.Distribution));
                            this.WriteData(Seats.West, ProtocolHelper.Translate(Seats.West, this.c.currentBoard.Distribution));
                            break;
                        case TableManagerProtocolState.WaitForCardPlay:
                            break;
                        case TableManagerProtocolState.WaitForOtherBid:
                            for (Seats s = Seats.North; s <= Seats.West; s++) this.clients[s].state = TableManagerProtocolState.WaitForCardPlay;
                            ProtocolHelper.HandleProtocolBid(this.lastRelevantMessage, this.EventBus);
                            break;
                        case TableManagerProtocolState.WaitForOtherCardPlay:
                            ProtocolHelper.HandleProtocolPlay(this.lastRelevantMessage, this.EventBus);
                            break;
                        case TableManagerProtocolState.WaitForOwnCardPlay:
                            break;
                        case TableManagerProtocolState.WaitForDummiesCardPlay:
                            break;
                        case TableManagerProtocolState.GiveDummiesCards:
                            var cards = ProtocolHelper.Translate(this.CurrentResult.Play.Dummy, this.c.currentBoard.Distribution).Replace(this.CurrentResult.Play.Dummy.ToXMLFull(), "Dummy");
                            for (Seats s = Seats.North; s <= Seats.West; s++)
                            {
                                if (s != this.CurrentResult.Play.Dummy)
                                {
                                    this.WriteData(s, cards);
                                }
                            }
                            for (Seats s = Seats.North; s <= Seats.West; s++)
                            {
                                this.clients[s].state = (s == this.CurrentResult.Auction.Declarer ? TableManagerProtocolState.WaitForOwnCardPlay : TableManagerProtocolState.WaitForCardPlay);
                                lock (this.clients) this.clients[s].Pause = false;
                            }
                            break;
                        case TableManagerProtocolState.WaitForDisconnect:
                            break;
                        case TableManagerProtocolState.WaitForLead:
                            break;
                        case TableManagerProtocolState.Finished:
                            break;
                        default:
                            break;
                    }
                }
            }
			else
            {
#if syncTrace
                Log.Trace(0, "Host expected '{0}'", expected);
                this.DumpQueue();
#endif
                this.Refuse(seat, "Expected '{0}'", expected);
                throw new InvalidOperationException(string.Format("Expected '{0}'", expected));
            }
        }

        public abstract void WriteData(Seats seat, string message, params object[] args);

		public virtual void Refuse(Seats seat, string reason, params object[] args)
		{
			this.WriteData(seat, reason, args);
		}

        public void BroadCast(string message, params object[] args)
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                this.WriteData(s, message, args);
            }
            this.lagTimer.Restart();
        }

        private void UpdateCommunicationLag(Seats source, long lag)
        {
#if syncTrace
#endif
            //Log.Trace("Host UpdateCommunicationLag for {0} old lag={1} lag={2}", source, this.clients[source].communicationLag, lag);
            this.clients[source].communicationLag += lag;
            this.clients[source].communicationLag /= 2;
            //Log.Trace("Host UpdateCommunicationLag for {0} new lag={1}", source, this.clients[source].communicationLag);
        }

        #region Bridge Events

        public override void HandleBoardStarted(int boardNumber, Seats dealer, Vulnerable vulnerabilty)
        {
#if syncTrace
            //Log.Trace("TableManagerHost.HandleBoardStarted");
#endif
            base.HandleBoardStarted(boardNumber, dealer, vulnerabilty);
            this.boardTime = new DirectionDictionary<TimeSpan>(new TimeSpan(), new TimeSpan());
            //Threading.Sleep(500);
            this.BroadCast("Start of board");
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                this.clients[s].Pause = false;
                this.clients[s].state = TableManagerProtocolState.WaitForStartOfBoard;
            }
        }

        public override void HandlePlayFinished(BoardResultRecorder currentResult)
        {
#if syncTrace
            Log.Trace(3, "HostBoardResult.HandlePlayFinished");
#endif
            base.HandlePlayFinished(currentResult);
            //Threading.Sleep(200);
            this.totalTime[Directions.NorthSouth] = this.totalTime[Directions.NorthSouth].Add(this.boardTime[Directions.NorthSouth]);
            this.totalTime[Directions.EastWest] = this.totalTime[Directions.EastWest].Add(this.boardTime[Directions.EastWest]);
            this.BroadCast("Timing - N/S : this board  {0:mm\\:ss},  total  {1:h\\:mm\\:ss}.  E/W : this board  {2:mm\\:ss},  total  {3:h\\:mm\\:ss}.", this.boardTime[Directions.NorthSouth].RoundToSeconds(), this.totalTime[Directions.NorthSouth].RoundToSeconds(), this.boardTime[Directions.EastWest].RoundToSeconds(), this.totalTime[Directions.EastWest].RoundToSeconds());
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                this.clients[s].state = TableManagerProtocolState.WaitForStartOfBoard;
                this.clients[s].Pause = false;
            }
        }

        public override void HandleTournamentStopped()
        {
#if syncTrace
            //Log.Trace("TableManagerHost.HandleTournamentStopped");
#endif
            this.BroadCast("End of session");
            this.moreBoards = false;
        }

#endregion

        private class TMController : TournamentController
        {
            private TableManagerHost host;

            public TMController(TableManagerHost h, Tournament t, ParticipantInfo p, BridgeEventBus bus) : base(t, p, bus)
            {
                this.host = h;
            }

            protected override BoardResultRecorder NewBoardResult(int boardNumber)
            {
                this.host.CurrentResult = new HostBoardResult(this.host, this.currentBoard, this.participant.PlayerNames.Names, this.EventBus);
                return this.host.CurrentResult;
            }
        }

        private class HostBoardResult : BoardResultEventPublisher
        {
            private TableManagerHost host;
            private System.Diagnostics.Stopwatch timer;

            public HostBoardResult(TableManagerHost h, Board2 board, SeatCollection<string> newParticipants, BridgeEventBus bus)
                : base("HostBoardResult", board, newParticipants, bus)
            {
                this.host = h;
                this.timer = new System.Diagnostics.Stopwatch();
            }

            public override void HandleBidNeeded(Seats whoseTurn, Bid lastRegularBid, bool allowDouble, bool allowRedouble)
            {
                base.HandleBidNeeded(whoseTurn, lastRegularBid, allowDouble, allowRedouble);
                timer.Restart();
            }

            public override void HandleBidDone(Seats source, Bid bid)
            {
                timer.Stop();
                this.host.boardTime[source.Direction()] = this.host.boardTime[source.Direction()].Add(timer.Elapsed.Subtract(new TimeSpan(this.host.clients[source].communicationLag)));
#if syncTrace
                //Log.Trace("HostBoardResult.HandleBidDone");
#endif
                base.HandleBidDone(source, bid);
                for (Seats s = Seats.North; s <= Seats.West; s++)
                {
                    this.host.clients[s].state = TableManagerProtocolState.WaitForMyCards;
                    if (s != source)
                    {
                        this.host.WriteData(s, ProtocolHelper.Translate(bid, source));
                    }
                }

                lock (this.host.clients) for (Seats s = Seats.North; s <= Seats.West; s++)
                {
                    this.host.clients[s].Pause = this.Auction.Ended;
                }
            }

            public override void HandleCardNeeded(Seats controller, Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
            {
#if syncTrace
                //Log.Trace("HostBoardResult.HandleCardNeeded");
#endif
                if (leadSuit == Suits.NoTrump)
                {
                    //Threading.Sleep(200);
                    this.host.WriteData(controller, "{0} to lead", whoseTurn == this.Play.Dummy ? "Dummy" : whoseTurn.ToXMLFull());
                }

                for (Seats s = Seats.North; s <= Seats.West; s++)
                {
                    this.host.clients[s].state = (s == controller ? TableManagerProtocolState.WaitForOwnCardPlay : TableManagerProtocolState.WaitForCardPlay);
                    lock (this.host.clients) this.host.clients[s].Pause = false;
                }

                timer.Restart();
            }

            public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
            {
                timer.Stop();
                this.host.boardTime[source.Direction()] = this.host.boardTime[source.Direction()].Add(timer.Elapsed.Subtract(new TimeSpan(this.host.clients[source].communicationLag)));
#if syncTrace
                Log.Trace(3, "HostBoardResult.HandleCardPlayed {0} plays {2}{1}", source, suit.ToXML(), rank.ToXML());
#endif
                base.HandleCardPlayed(source, suit, rank);
                for (Seats s = Seats.North; s <= Seats.West; s++)
                {
                    if ((s != source && !(s == this.Auction.Declarer && source == this.Play.Dummy))
                        || (s == source && source == this.Play.Dummy)
                        )
                    {
                        this.host.WriteData(s, "{0} plays {2}{1}", source, suit.ToXML(), rank.ToXML());
                    }

                    if (this.Play.currentTrick == 1 && this.Play.man == 2)
                    {   // 1st card: need to send dummies cards
#if syncTrace
                        //Log.Trace("HostBoardResult.HandleCardPlayed 1st card to {0}", s);
#endif
                        var mustPause = s == this.Play.Dummy;
                        lock (this.host.clients) this.host.clients[s].Pause = mustPause;
                        this.host.clients[s].state = s == this.Play.Dummy ? TableManagerProtocolState.GiveDummiesCards : TableManagerProtocolState.WaitForDummiesCards;
                    }
                }
            }

            public override void HandleNeedDummiesCards(Seats dummy)
            {
                //base.HandleNeedDummiesCards(dummy);
            }

            public override void HandleShowDummy(Seats dummy)
            {
                //base.HandleShowDummy(dummy);
            }
        }
    }

    public static class x
    {
        public static TimeSpan RoundToSeconds(this TimeSpan timespan, int seconds = 1)
        {
            long offset = (timespan.Ticks >= 0) ? TimeSpan.TicksPerSecond / 2 : TimeSpan.TicksPerSecond / -2;
            return TimeSpan.FromTicks((timespan.Ticks + offset) / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
        }
    }
}
