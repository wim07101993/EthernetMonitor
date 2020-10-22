using System;
using System.IO;

using WSharp.Logging;
using WSharp.Logging.Loggers;

namespace EthernetMonitor
{
    /// <summary>Logs to the <see cref="Console"/>.</summary>
    public class ConsoleLogger : ALogger
    {
        #region FIELDS

        private TextWriter _writer;

        #endregion FIELDS

        public ConsoleLogger()
        {
        }

        public ConsoleLogger(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _writer = new StreamWriter(stream);
        }

        public ConsoleLogger(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public TextWriter Writer
        {
            get
            {
                _ = EnsureWriter();
                return _writer;
            }
            set => _writer = value;
        }

        protected bool IsWriterNull => _writer == null;

        public override void Dispose(bool isDisposing)
        {
            try
            {
                // clean up resources
                if (_writer != null)
                {
                    try
                    {
                        _writer.Close();
                    }
                    catch (ObjectDisposedException) { }
                }
                _writer = null;
            }
            finally
            {
                base.Dispose(isDisposing);
            }
        }

        protected override void InternalLog(ILogEntry logEntry)
        {
            if (!EnsureWriter())
                return;

            try
            {
                _writer.Write(logEntry);
                _writer.Flush();
            }
            catch (ObjectDisposedException) { }
        }

        protected bool EnsureWriter()
        {
            if (!IsWriterNull)
                return true;

            Writer = Console.Out;
            return true;
        }
    }
}
