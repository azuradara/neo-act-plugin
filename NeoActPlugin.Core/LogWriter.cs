using Advanced_Combat_Tracker;
using NeoActPlugin.Common;
using System;
using System.IO;
using System.Threading;

namespace NeoActPlugin.Core
{
    public static class LogWriter
    {
        private static Thread _workerThread;
        private static volatile bool _stopRequested;
        private static string _logFilePath;
        private static StreamWriter _logWriter;

        public static void Initialize()
        {
            try
            {
                _stopRequested = false;
                _workerThread = new Thread(WorkerMain) { IsBackground = true, Priority = ThreadPriority.BelowNormal };

                var logDir = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "BNSLogs");
                Directory.CreateDirectory(logDir);

                _logFilePath = Path.Combine(logDir, string.Format("combatlog_{0}.log", DateTime.Now.ToString("yyyy-MM-dd")));
                _logWriter = new StreamWriter(_logFilePath, append: true) { AutoFlush = true };

                _logWriter.WriteLine(string.Empty);

                ActGlobals.oFormActMain.LogFilePath = _logFilePath;
                ActGlobals.oFormActMain.OpenLog(false, false);

                _workerThread.Start();
            }
            catch (Exception ex)
            {
                LogError("Initialize", ex);
                _stopRequested = true;
            }
        }

        public static void Uninitialize()
        {
            try
            {
                if (_workerThread != null)
                {
                    _stopRequested = true;

                    for (int i = 0; i < 10; i++)
                    {
                        if (_workerThread.ThreadState == System.Threading.ThreadState.Stopped)
                            break;
                        System.Threading.Thread.Sleep(50);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (_workerThread.ThreadState != System.Threading.ThreadState.Stopped)
                        _workerThread.Abort();

                    _workerThread = null;

                    _logWriter.Dispose();
                }
            }
            catch (Exception ex)
            {
                PluginMain.WriteLog(LogLevel.Error, "Error [BNS_Log.Uninitialize] " + ex.ToString().Replace(Environment.NewLine, " "));
            }
        }

        private static void WorkerMain()
        {
            var reader = new Reader();

            while (!_stopRequested)
            {
                try
                {
                    foreach (var result in reader.Read())
                    {
                        if (_stopRequested) break;

                        var message = string.Format("{0}|{1}", DateTime.Now.ToString("HH:mm:ss.fff"), result);
                        _logWriter.WriteLine(message);
                    }

                    Thread.Sleep(14);
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }

        }

        private static void LogError(string context, Exception ex)
        {
            try
            {
                string errorMessage = string.Format("{0}|Error [{1}] {2}", DateTime.Now.ToString("HH:mm:ss.fff"), context, ex.ToString());
                _logWriter.WriteLine(_logFilePath, errorMessage);
                PluginMain.WriteLog(LogLevel.Error, errorMessage);
            }
            catch { /* Prevent logging failures from crashing thread */ }
        }
    }
}
