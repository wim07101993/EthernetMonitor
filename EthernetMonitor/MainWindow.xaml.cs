using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace EthernetMonitor
{
    public partial class MainWindow : Window
    {
        private bool _autoScroll = true;
        private bool _isAutoScrolling;

        public MainWindow()
        {
            InitializeComponent();
            App.ConsoleOutput.CollectionChanged += OnConsoleOutputChanged;
        }

        private void OnConsoleOutputScrollviewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isAutoScrolling)
                return;

            _autoScroll = ConsoleOutputScrollviewer.VerticalOffset == ConsoleOutputScrollviewer.ScrollableHeight;
        }

        private void OnConsoleOutputChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_autoScroll)
                return;

            _isAutoScrolling = true;
            ConsoleOutputScrollviewer.ScrollToVerticalOffset(ConsoleOutputScrollviewer.ExtentHeight);
            _isAutoScrolling = false;
        }
    }
}
