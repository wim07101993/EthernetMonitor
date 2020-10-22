using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using WSharp.Logging.Loggers;

namespace EthernetMonitor
{
    public class Server : AConnectionBase
    {
        private TcpListener _listener;

        public Server(ILogger logger)
            : base(logger)
        {
            PropertyChanged += OnPropertyChagned;
        }

        public bool IsWaitingForClient
        {
            get => IsConnecting;
            set => IsConnecting = value;
        }

        public override async Task ConnectTcpAsync()
        {
            IsWaitingForClient = true;
            try
            {
                _listener = new TcpListener(
                    string.IsNullOrWhiteSpace(HostName) ? IPAddress.Any : IPAddress.Parse(HostName),
                    PortNumber);
                _listener.Start();
                Tcp = await _listener.AcceptTcpClientAsync();

                _ = StartReceivingAsync();
            }
            finally
            {
                IsConnected = Tcp?.Connected ?? false;
                IsWaitingForClient = false;
            }
        }

        public override Task ConnectUdpAsync() 
            => throw new NotImplementedException("This feature is not implemented yet...");

        public override async Task DisconnectAsync()
        {
            await base.DisconnectAsync();

            _listener?.Stop();
            _listener = null;
        }

        private void OnPropertyChagned(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsConnecting):
                    RaisePropertyChanged(nameof(IsWaitingForClient));
                    break;
            }
        }
    }
}
