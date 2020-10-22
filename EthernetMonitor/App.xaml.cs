using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

using WSharp.Logging.Loggers;

namespace EthernetMonitor
{
    public partial class App : Application
    {
        public static ILogger Logger { get; }

        public static ObservableTextReader ConsoleOutput { get; } = new ObservableTextReader();

        static App()
        {
            Console.SetOut(ConsoleOutput);
            Logger = new ConsoleLogger(ConsoleOutput);
        }

        public App()
        {
             DispatcherUnhandledException += OnUnhandledExceptoin;
        }

        private void OnUnhandledExceptoin(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Log(args.Exception, TraceEventType.Error, "Unhandled exception");
            args.Handled = true;
        }

        protected void Log(object o, TraceEventType eventType = TraceEventType.Verbose, [CallerMemberName] string tag = null)
           => Logger?.Log(GetType().Name, o, eventType, tag);

        protected void Log<T>(string title, T payload, TraceEventType eventType = TraceEventType.Verbose, [CallerMemberName] string tag = null)
            where T : class
            => Logger?.Log(source: GetType().Name, title: title, payload: new[] { payload }, eventType: eventType, tag: tag);
    }
}
