using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sodes.Bridge.Base;
using Sodes.Bridge.Networking;

namespace TestServerConsole
{
  class Program
  {
    private static BridgeEventBus _hostEventBus;
    static ManualResetEventSlim _waiter = new ManualResetEventSlim(false);

    static void Main(string[] args)
    {
      SetupServer();
      _waiter.Wait();
    }

    private static void SetupServer()
    {
      _hostEventBus = new BridgeEventBus("TM_Host");
      var host = new TableManagerTcpHost(2000, _hostEventBus);
      host.OnHostEvent += Host_OnHostEvent;
    }

    private static void Host_OnHostEvent(TableManagerHost sender, HostEvents hostEvent, Seats seat, string message)
    {
      Console.WriteLine(Enum.GetName(typeof(HostEvents),hostEvent));
      switch (hostEvent)
      {
        case HostEvents.ReadyForTeams:
          Console.WriteLine("Starting tournament...");
          sender.HostTournament("WC2005final01.pbn");
          Console.WriteLine("done.");
          break;
        case HostEvents.ReadyToStart:
          Console.WriteLine("ready to exit.");
          _waiter.Set();
          break;
        default:
          Console.WriteLine("no action taken.");
          break;
      }
    }
  }
}
