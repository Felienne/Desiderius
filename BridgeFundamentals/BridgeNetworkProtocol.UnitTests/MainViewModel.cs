using Sodes.Bridge.Base;
using Sodes.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoboBridge.TableManager.Client.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                var programArguments = args[1].Split('&');
                var arguments = new Dictionary<string, string>();
                for (int i = 0; i < programArguments.Length; i++)
                {
                    var x = programArguments[i].Split('=');
                    arguments.Add(x[0].ToLower(), x[1]);
                }

                Task.Run(() =>
                {
                    this.Connect(
                        SeatsExtensions.FromXML(arguments["seat"])
                        , arguments["ipaddress"]
                        , int.Parse(arguments["port"])
                        , 120
                        , 60
                        , arguments["networkname"]
                        , 1
                        , true    // send alerts to tablemanager?
                        );
                });
            }
        }

        private Seats mySeat;
        private TableManagerTcpClient connectionManager;

        private bool _sessionEnd = false;
        public bool SessionEnd
        {
            get
            {
                return _sessionEnd;
            }

            set
            {
                if (_sessionEnd != value)
                {
                    _sessionEnd = value;
                    RaisePropertyChanged(nameof(SessionEnd));
                }
            }
        }

        public void Connect(Seats _seat, string serverName, int portNumber, int _maxTimePerBoard, int _maxTimePerCard, string teamName, int botCount, bool _sendAlerts)
        {
            this.mySeat = _seat;
            var bus = new BridgeEventBus("TM_Client " + _seat);
            var bot = new ChampionshipRobot(this.mySeat, bus);
            bus.HandleTournamentStarted(Scorings.scIMP, _maxTimePerBoard, _maxTimePerCard, "");
            bus.HandleRoundStarted(new SeatCollection<string>(), new DirectionDictionary<string>("RoboBridge", "RoboBridge"));
            this.connectionManager = new TableManagerTcpClient(bus);
            this.connectionManager.OnBridgeNetworkEvent += ConnectionManager_OnBridgeNetworkEvent;
            this.connectionManager.Connect(_seat, serverName, portNumber, _maxTimePerBoard, _maxTimePerCard, teamName, botCount, _sendAlerts);
        }

        private void ConnectionManager_OnBridgeNetworkEvent(object sender, BridgeNetworkEvents e, BridgeNetworkEventData data)
        {
            switch (e)
            {
                case BridgeNetworkEvents.Seated:
                    break;
                case BridgeNetworkEvents.Teams:
                    break;
                case BridgeNetworkEvents.Error:
                    break;
                case BridgeNetworkEvents.SessionEnd:
                    this.SessionEnd = true;
                    break;
                default:
                    break;
            };
        }

        private class ChampionshipRobot : BridgeFundamentals.UnitTests.TestRobot
        {
            private Scorings scoring;
            private int maxTimePerBoard;
            private int maxTimePerCard;
            private string tournamentName;

            public ChampionshipRobot(Seats seat, BridgeEventBus bus) : base(seat, bus)
            {
            }

            public override void HandleTournamentStarted(Scorings _scoring, int _maxTimePerBoard, int _maxTimePerCard, string _tournamentName)
            {
                this.scoring = _scoring;
                this.maxTimePerBoard = _maxTimePerBoard;
                this.maxTimePerCard = _maxTimePerCard;
                this.tournamentName = _tournamentName;
            }

            public override Bid FindBid(Bid lastRegularBid, bool allowDouble, bool allowRedouble)
            {
                //TODO: implement your own logic
                return base.FindBid(lastRegularBid, allowDouble, allowRedouble);
            }

            public override Card FindCard(Seats whoseTurn, Suits leadSuit, Suits trump, bool trumpAllowed, int leadSuitLength, int trick)
            {
                //TODO: implement your own logic
                //Thread.Sleep(1000);
                return base.FindCard(whoseTurn, leadSuit, trump, trumpAllowed, leadSuitLength, trick);
            }
        }
    }

    public class ViewModelBase
    {
        protected void RaisePropertyChanged(string v)
        {
        }
    }
}