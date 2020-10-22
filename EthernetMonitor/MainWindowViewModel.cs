using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;

namespace EthernetMonitor
{
    public class MainWindowViewModel : BindableBase
    {
        private ICommand _connectCommand;
        private ICommand _disconnectCommand;

        private bool _isServerEnabled = true;
        private bool _isClientEnabled = true;
        private bool _isAutoSendEnabled = true;
        private bool _isConnecting;
        private bool _isDisconnecting;

        public MainWindowViewModel()
        {
            Client = new Client(App.Logger);
            Server = new Server(App.Logger);

            Server.PropertyChanged += OnServerPropertyChanged;
            Client.PropertyChanged += OnClientPropertyChanged;
        }

        public Client Client { get; }
        public Server Server { get; }

        public ObservableTextReader ConsoleOutput => App.ConsoleOutput;

        public bool IsServerEnabled
        {
            get => _isServerEnabled;
            set => SetProperty(ref _isServerEnabled, value);
        }

        public bool IsClientEnabled
        {
            get => _isClientEnabled;
            set => SetProperty(ref _isClientEnabled, value);
        }

        public bool IsAutoSendEnabled
        {
            get => _isAutoSendEnabled;
            set => SetProperty(ref _isAutoSendEnabled, value);
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            private set => SetProperty(ref _isConnecting, value);
        }

        public bool IsDisconnecting
        {
            get => _isDisconnecting;
            private set => SetProperty(ref _isDisconnecting, value);
        }

        public ICommand ConnectCommand => _connectCommand ??= new DelegateCommand(async () =>
        {
            IsConnecting = true;

            Task serverConnectingTask = null;
            Task clientConnectingTask = null;

            if (IsServerEnabled)
                serverConnectingTask = Server.ConnectAsync();
            if (IsClientEnabled)
                clientConnectingTask = Client.ConnectAsync();

            if (IsServerEnabled)
                await serverConnectingTask;
            if (IsClientEnabled)
                await clientConnectingTask;

            IsConnecting = false;
        });

        public ICommand DisconnectCommand => _disconnectCommand ??= new DelegateCommand(async () =>
        {
            IsDisconnecting = true;

            var serverConnectingTask = Server.DisconnectAsync();
            var clientConnectingTask = Client.DisconnectAsync();

            await serverConnectingTask;
            await clientConnectingTask;

            IsDisconnecting = false;
        });

        private void OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AConnectionBase.LastReceivedMessage):
                    if (IsAutoSendEnabled && IsServerEnabled)
                    {
                        Server.MessageToSend = new Message { Data = Client.LastReceivedMessage };
                        _ = Server.SendAsync();
                    }
                    break;
            }
        }

        private void OnServerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AConnectionBase.LastReceivedMessage):
                    if (IsAutoSendEnabled && IsClientEnabled)
                    {
                        Client.MessageToSend = new Message { Data = Server.LastReceivedMessage };
                        _ = Client.SendAsync();
                    }
                    break;
            }
        }
    }
}
