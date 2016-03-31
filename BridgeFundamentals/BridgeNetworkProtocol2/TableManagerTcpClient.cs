using Sodes.Base;
using Sodes.Bridge.Base;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Sodes.Bridge.Networking
{
    public class TableManagerTcpClient : TableManagerClient, IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] streamBuffer;        // buffer for raw async NetworkStream
        private string rawMessageBuffer;		// String to store the response ASCII representation.
        private object locker = new object();

        public TableManagerTcpClient() : this(null) { }

        public TableManagerTcpClient(BridgeEventBus bus) : base(bus)
        {
        }

        public void Connect(Seats _seat, string serverName, int portNumber, int _maxTimePerBoard, int _maxTimePerCard, string teamName, int botCount, bool _sendAlerts)
        {
            Log.Trace(2, "Open connection to {0}:{1}", serverName, portNumber);
            // Create a TcpClient.
            client = new TcpClient(serverName, portNumber);
            this.client.NoDelay = true;   // make sure that data is sent immediately to TM
            this.client.ReceiveTimeout = 30;
            stream = client.GetStream();
            this.streamBuffer = new Byte[this.client.ReceiveBufferSize];
            this.rawMessageBuffer = "";    // initialize the response buffer

            this.WaitForTcpData();
            base.Connect(_seat, _maxTimePerBoard, _maxTimePerCard, teamName, botCount, _sendAlerts);
        }

        protected override async Task WriteProtocolMessageToRemoteMachine(string message)
        {
            Log.Trace(0, "TM {1} sends '{0}'", message, this.seat.ToString().PadRight(5));

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\r\n");    // newline is required for TableManager protocol

            try
            {
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();      // Send the message to the connected TcpServer (without Flush the message will stay in the buffer) 
            }
            catch (System.IO.IOException x)
            {
                Log.Trace(0, "Error '{0}'", x.Message);
            }
        }

        private void ProcessRawMessage()
        {
            string newCommand = "";
            lock (this.locker)
            {
                int endOfLine = rawMessageBuffer.IndexOf("\r\n");
                if (endOfLine >= 0)
                {
                    newCommand = this.rawMessageBuffer.Substring(0, endOfLine);
                    this.rawMessageBuffer = this.rawMessageBuffer.Substring(endOfLine + 2);
                }
            }

            if (newCommand.Length > 0) this.ProcessIncomingMessage(newCommand);
        }

        private void WaitForTcpData()
        {
            // make sure no messages get lost; go wait for another message on the tcp line
            this.stream.BeginRead(this.streamBuffer, 0, this.client.ReceiveBufferSize, new AsyncCallback(this.ReadData), null);
        }

        private void ReadData(IAsyncResult result)
        {
            int bytes2 = this.stream.EndRead(result);
            if (bytes2 > 0)
            {
                string newData = System.Text.Encoding.ASCII.GetString(this.streamBuffer, 0, bytes2);
                lock (this.locker)
                {
                    this.rawMessageBuffer += newData;
                }

                this.ProcessRawMessage();
                this.WaitForTcpData();		// make sure no data will be lost
            }
        }

        //public override bool IsConnected()
        //{
        //    return this.stream != null && this.stream.CanRead;
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TableManagerTcpClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.client != null)
                {
                    this.client.Close();
                    this.client = null;
                }
            }
        }
    }

    public class BridgeNetworkErrorEventData : BridgeNetworkEventData
    {
        public string Error;

        public BridgeNetworkErrorEventData(string _error)
        {
            this.Error = _error;
        }
    }
}
