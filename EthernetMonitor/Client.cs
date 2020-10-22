using System.Net.Sockets;
using System.Threading.Tasks;

using WSharp.Logging.Loggers;

namespace EthernetMonitor
{
    public class Client : AConnectionBase
    {
        public Client(ILogger logger)
            : base(logger)
        {
        }

        public override async Task ConnectTcpAsync()
        {
            var client = new TcpClient();
            await client.ConnectAsync(HostName, PortNumber);
            Tcp = client;
        }

        public override Task ConnectUdpAsync() => Task.Run(() =>
        {
            var client = new UdpClient();
            client.Connect(HostName, PortNumber);
            Udp = client;

            _ = StartReceivingAsync();
        });
    }
}
