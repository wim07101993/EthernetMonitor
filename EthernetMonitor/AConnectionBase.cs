using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;

using WSharp.Logging.Loggers;

namespace EthernetMonitor
{
    public abstract class AConnectionBase : BindableBase
    {
        #region FIELDS

        private TcpClient _tcp;
        private UdpClient _udp;

        private CancellationTokenSource _cts;

        private ushort _portNumber;
        private string _address;
        private EProtocol _protocol = EProtocol.Tcp;

        private byte[] _lastReceivedMessage;
        private Message _messageToSend = new Message();

        private bool _isConnecting;
        private bool _isConnected;
        private bool _isReceiving;
        private bool _isSending;

        private ICommand _connectCommand;
        private ICommand _disconnectCommand;
        private ICommand _sendCommand;
        private bool _isDisconnecting;

        #endregion FIELDS

        public AConnectionBase(ILogger logger)
        {
            Logger = logger;
        }

        #region PROPERTIES

        protected TcpClient Tcp
        {
            get => _tcp;
            set
            {
                if (!SetProperty(ref _tcp, value))
                    return;

                RaisePropertyChanged(nameof(Socket));
            }
        }

        protected UdpClient Udp
        {
            get => _udp;
            set
            {
                if (!SetProperty(ref _udp, value))
                    return;

                RaisePropertyChanged(nameof(Socket));
            }
        }

        protected Socket Socket => Protocol switch
        {
            EProtocol.Tcp => _tcp?.Client,
            EProtocol.Udp => _udp?.Client,
            _ => throw new NotImplementedException("This feature is not implemented yet...")
        };

        public string HostName
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public ushort PortNumber
        {
            get => _portNumber;
            set => SetProperty(ref _portNumber, value);
        }

        public EProtocol Protocol
        {
            get => _protocol;
            set
            {
                if (!SetProperty(ref _protocol, value))
                    return;

                _ = DisconnectAsync();
            }
        }

        public byte[] LastReceivedMessage
        {
            get => _lastReceivedMessage;
            set => SetProperty(ref _lastReceivedMessage, value);
        }

        public Message MessageToSend
        {
            get => _messageToSend;
            set => SetProperty(ref _messageToSend, value);
        }

        public ObservableCollection<ReadOnlyMessage> ReceivedMessages { get; } = new ObservableCollection<ReadOnlyMessage>();
        public ObservableCollection<ReadOnlyMessage> SentMessages { get; } = new ObservableCollection<ReadOnlyMessage>();

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if (!SetProperty(ref _isConnecting, value))
                    return;

                if (value)
                    Log("Connecting");
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (!SetProperty(ref _isConnected, value))
                    return;

                if (value)
                    Log("Connected");
                else
                    Log("Disconnected");
            }
        }

        public bool IsSending
        {
            get => _isSending;
            set => SetProperty(ref _isSending, value);
        }

        public bool IsReceiving
        {
            get => _isReceiving;
            set => SetProperty(ref _isReceiving, value);
        }

        public bool IsDisconnecting
        {
            get => _isDisconnecting;
            set
            {
                if (!SetProperty(ref _isDisconnecting, value))
                    return;

                if (value)
                    Log("Disconnecting");
            }
        }

        public ICommand ConnectCommand => _connectCommand ??= new DelegateCommand(() => _ = ConnectAsync());

        public ICommand DisonnectCommand => _disconnectCommand ??= new DelegateCommand(() => _ = DisconnectAsync());

        public ICommand SendCommand => _sendCommand ??= new DelegateCommand(() => _ = SendAsync());

        #endregion PROPERTIES

        #region METHODS

        public Task ConnectAsync()
            => Task.Run(() => TryAsync(async () =>
        {
            await DisconnectAsync();

            IsConnecting = true;
            _cts = new CancellationTokenSource();

            switch (Protocol)
            {
                case EProtocol.Tcp:
                    await ConnectTcpAsync();
                    break;

                case EProtocol.Udp:
                    await ConnectUdpAsync();
                    break;

                default:
                    throw new NotImplementedException("Sorry this feature is not available yet...");
            }

            IsConnected = Socket.Connected;
            IsConnecting = false;

            _ = StartReceivingAsync();
        }));

        public virtual Task DisconnectAsync()
             => TryAsync(() => Task.Run(() =>
        {
            IsDisconnecting = true;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _tcp?.Dispose();
            _tcp = null;
            _udp?.Dispose();
            _udp = null;

            IsConnected = false;
        }));

        public abstract Task ConnectTcpAsync();

        public abstract Task ConnectUdpAsync();

        protected Task StartReceivingAsync()
            => Task.Factory.StartNew(() => TryAsync(() =>
            {
            var socket = Socket;
            if (socket == null || !IsConnected)
                throw new InvalidOperationException("First connect, then receive messages");

            IsReceiving = true;
            while (!_cts.IsCancellationRequested && IsSocketConnected())
            {
                try
                {
                    var data = new byte[512];
                    _ = socket.Receive(data);
                    data = data.Where(x => x > 0).ToArray();
                    LastReceivedMessage = data;
                    var message = new ReadOnlyMessage(data);
                    Application.Current.Dispatcher.Invoke(() => ReceivedMessages.Add(message));
                    Log($"Received message", new 
                    { 
                        message.Ascii, 
                        message.Unicode, 
                        Data = message.Data.Aggregate(new StringBuilder(), (b, x) => b.Append($"{x:X} ")).ToString()
                    });
                }
                catch (Exception e)
                {
                    var data = Encoding.ASCII.GetBytes($"An error happened: {e.Message}");
                    Application.Current.Dispatcher.Invoke(() => ReceivedMessages.Add(new ReadOnlyMessage(data)));
                    Log("An error happened", e);
                }
            }

            IsReceiving = false;

            if (!socket.Connected)
                IsConnected = false;

                return Task.CompletedTask;
        }), TaskCreationOptions.LongRunning);

        public Task SendAsync()
            => TryAsync(() => Task.Run(() =>
        {
            var socket = Socket;
            if (socket == null || !IsConnected)
                throw new InvalidOperationException("First connect, then send messages");

            IsSending = true;
            try
            {
                _ = socket.Send(MessageToSend.Data);
                var message = new ReadOnlyMessage(MessageToSend.Data);
                Application.Current.Dispatcher.Invoke(() => SentMessages.Add(message));
                Log($"Sent message", new
                {
                    message.Ascii,
                    message.Unicode,
                    Data = message.Data.Aggregate(new StringBuilder(), (b, x) => b.Append($"{x:X} ")).ToString()
                });
            }
            catch (Exception e)
            {
                var data = Encoding.ASCII.GetBytes($"An error happened: {e.Message}");
                Application.Current.Dispatcher.Invoke(() => ReceivedMessages.Add(new ReadOnlyMessage(data)));
                Log("An error happened", e);
            }
        }));

        public bool IsSocketConnected()
        {
            var socket = Socket;
            return !socket.Poll(1000, SelectMode.SelectRead) | socket.Available != 0;
        }

        protected async Task TryAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                Log($"Error: {e.Message}", e);
            }
        }

        #endregion METHODS

        #region Logging

        protected ILogger Logger { get; }

        protected void Log(object o = null, TraceEventType eventType = TraceEventType.Verbose, [CallerMemberName] string tag = null)
           => Logger?.Log(GetType().Name, o, eventType, tag);

        protected void Log<T>(string title, T payload, TraceEventType eventType = TraceEventType.Verbose, [CallerMemberName] string tag = null)
            where T : class
            => Logger?.Log(source: GetType().Name, title: title, payload: new[] { payload }, eventType: eventType, tag: tag);

        protected void Log<T>(string title, object[] payload, TraceEventType eventType = TraceEventType.Verbose, [CallerMemberName] string tag = null)
            where T : class
            => Logger?.Log(source: GetType().Name, title: title, payload: payload, eventType: eventType, tag: tag);

        #endregion Logging
    }
}
