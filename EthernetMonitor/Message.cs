using System.Text;

using Prism.Mvvm;

namespace EthernetMonitor
{
    public class Message : BindableBase
    {
        private byte[] _data = new byte[0];

        public Message()
        {
        }

        public string Ascii
        {
            get => Encoding.ASCII.GetString(_data);
            set => Data = Encoding.ASCII.GetBytes(value);
        }
        public string Unicode
        {
            get
            {
                return Encoding.Unicode.GetString(Data);
            }
            set => Data = Encoding.Unicode.GetBytes(value);
        }

        public byte[] Data
        {
            get => _data;
            set
            {
                if (!SetProperty(ref _data, value))
                    return;

                RaisePropertyChanged(nameof(Ascii));
                RaisePropertyChanged(nameof(Unicode));
            }
        }
    }
}
