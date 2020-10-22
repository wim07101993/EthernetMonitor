using System.Text;

namespace EthernetMonitor
{
    public class ReadOnlyMessage
    {
        public ReadOnlyMessage(byte[] data)
        {
            Data = data;
        }

        public string Ascii => Encoding.ASCII.GetString(Data);
        public string Unicode => Encoding.Unicode.GetString(Data);

        public byte[] Data { get; }
    }
}
