using NeoActPlugin.Common;
using System;
using System.ComponentModel;

namespace NeoActPlugin.Core
{
    public class Logger : ILogger
    {
        public BindingList<LogEntry> Logs { get; private set; }
        private Action<LogEntry> listener = null;

        public Logger()
        {
            this.Logs = new BindingList<LogEntry>();
        }

        public void Log(LogLevel level, string message)
        {
#if !DEBUG
            if (level == LogLevel.Trace || level == LogLevel.Debug)
            {
                return;
            }
#endif
#if DEBUG
            System.Diagnostics.Trace.WriteLine(string.Format("{0}: {1}: {2}", level, DateTime.Now, message));
#endif

            var entry = new LogEntry(level, DateTime.Now, message);

            lock (Logs)
            {

                if (listener != null)
                {
                    listener(entry);
                }
                else
                {
                    Logs.Add(entry);
                }
            }
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            Log(level, string.Format(format, args));
        }

        public void RegisterListener(Action<LogEntry> listener)
        {
            lock (Logs)
            {
                foreach (var entry in Logs)
                {
                    listener(entry);
                }

                this.listener = listener;
                this.Logs.Clear();
            }
        }

        public void ClearListener()
        {
            lock (Logs)
            {
                this.listener = null;
            }
        }
    }
}
