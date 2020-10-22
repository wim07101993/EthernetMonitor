using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;

namespace EthernetMonitor
{
    public class ObservableTextReader : TextWriter, INotifyCollectionChanged, IReadOnlyList<string>
    {
        private readonly StringBuilder _buffer = new StringBuilder();
        private string _endlineBuff = "";

        public override Encoding Encoding => Encoding.UTF8;

        public ObservableCollection<string> Contents { get; }

        public int Count => Contents.Count;

        public string this[int index] => Contents[index];

        public ObservableTextReader()
        {
            Contents = new ObservableCollection<string>();
            Contents.CollectionChanged += OnContentsChanged;
        }

        public override void Write(char value)
        {
            if (NewLine.Contains(value))
            {
                _endlineBuff += value;
                if (_endlineBuff == NewLine)
                {
                    Contents.Add(_buffer.ToString());
                    _ = _buffer.Clear();
                }
                else if (!NewLine.Contains(_endlineBuff))
                {
                    _ = _buffer.Append(_endlineBuff);
                    _endlineBuff = "";
                    _ = _buffer.Append(value);
                }
            }
            else
            {
                _ = _buffer.Append(_endlineBuff);
                _endlineBuff = "";
                _ = _buffer.Append(value);
            }

        }

        public override void WriteLine()
        {
            Contents.Add(_buffer.ToString());
            _ = _buffer.Clear();
        }

        public override void WriteLine(string value)
        {
            Contents.Add(_buffer.Append(value).ToString());
            _ = _buffer.Clear();
        }

        private void OnContentsChanged(object sender, NotifyCollectionChangedEventArgs e)
            => Application.Current.Dispatcher.Invoke(() => CollectionChanged?.Invoke(this, e));

        public IEnumerator<string> GetEnumerator() => Contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Contents.GetEnumerator();

        public void Clear() => Contents.Clear();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
