using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace Server.Services
{
    public class ClientSession : TcpSession
    {
        private ILogger<ClientSession> _logger;

        public ClientSession(TcpServer server, ILogger<ClientSession> logger) : base(server)
        {
            _logger = logger;
        }

        protected override void OnConnected()
        {
            _logger.Log(LogLevel.Information, $"Chat TCP session with Id {Id} connected!");

            // Send invite message
            const string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            _logger.Log(LogLevel.Information, $"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            _logger.Log(LogLevel.Information, "Incoming: " + message);

            // Multicast message to all connected sessions
            Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            _logger.Log(LogLevel.Information, $"Chat TCP session caught an error with code {error}");
        }
    }
}
