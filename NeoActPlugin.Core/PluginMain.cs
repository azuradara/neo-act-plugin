using Advanced_Combat_Tracker;
using NeoActPlugin.Common;
using System;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace NeoActPlugin.Core
{
    public class PluginMain
    {
        private TinyIoCContainer _container;
        private static ILogger _logger;

        TabPage tab;
        Label label;
        ControlPanel panel;

        internal string PluginDirectory { get; private set; }

        public PluginMain(string pluginDirectory, Logger logger, TinyIoCContainer container)
        {
            _container = container;
            PluginDirectory = pluginDirectory;
            _logger = logger;

            //configSaveTimer = new Timer();
            //configSaveTimer.Interval = 300000;
            //configSaveTimer.Tick += (o, e) => SaveConfig();

            _container.Register(this);
        }

        public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
        {
            try
            {
                this.tab = pluginScreenSpace;
                this.label = pluginStatusText;

                this.label.Text = "Initializing...";

                if (!IsRunningAsAdmin())
                {
                    this.label.Text = "Error: Run ACT as Administrator.";

                    MessageBox.Show(
                        "NeoActPlugin requires ACT to be run as Administrator. Please restart ACT with elevated privileges.",
                        "Admin Rights Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    this.DeInitPlugin();

                    return;
                }

                this.panel = new ControlPanel(_container);
                this.panel.Dock = DockStyle.Fill;
                this.tab.Controls.Add(this.panel);
                this.tab.Name = "Neo ACT Plugin";

                _logger.Log(LogLevel.Info, "Initialized.");

                Updater.Updater.PerformUpdateIfNecessary(PluginDirectory, _container);

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.UpdateCheckClicked += new Advanced_Combat_Tracker.FormActMain.NullDelegate(UpdateCheckClicked);
                if (Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                {
                    Thread updateThread = new Thread(new ThreadStart(UpdateCheckClicked));
                    updateThread.IsBackground = true;
                    updateThread.Start();
                }

                UpdateACTTables();

                LogParser.Initialize(new ACTWrapper());

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogPathHasCharName = false;
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogFileFilter = "*.log";

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.TimeStampLen = DateTime.Now.ToString("HH:mm:ss.fff").Length + 1;

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetDateTimeFromLog = new FormActMain.DateTimeLogParser(LogParser.ParseLogDateTime);

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(LogParser.BeforeLogLineRead);

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.ChangeZone("Blade & Soul");

                LogWriter.Initialize();

                this.label.Text = "Initialized.";
            }
            catch (Exception ex)
            {
                ActGlobals.oFormActMain.WriteInfoLog(ex.Message);
                WriteLog(LogLevel.Error, "Exception during InitPlugin: " + ex.ToString().Replace(Environment.NewLine, " "));
                this.label.Text = "InitPlugin Error.";
            }
        }

        private bool IsRunningAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void DeInitPlugin()
        {
            LogWriter.Uninitialize();

            Advanced_Combat_Tracker.ActGlobals.oFormActMain.UpdateCheckClicked -= this.UpdateCheckClicked;
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead -= LogParser.BeforeLogLineRead;

            if (this.label != null)
            {
                this.label.Text = "BnS Plugin Unloaded.";
                this.label = null;
            }
        }


        public void UpdateCheckClicked()
        {

        }

        private void UpdateACTTables()
        {

        }


        public static void WriteLog(LogLevel level, string message)
        {
            _logger.Log(level, message);
        }

        private void cmdClearMessages_Click(object sender, EventArgs e)
        {
            //lstMessages.Items.Clear();
        }

        private void cmdCopyProblematic_Click(object sender, EventArgs e)
        {
            //StringBuilder sb = new StringBuilder();
            //foreach (object itm in lstMessages.Items)
            //    sb.AppendLine((itm ?? "").ToString());

            //if (sb.Length > 0)
            //    System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }
    }

    public interface IACTWrapper
    {
        bool SetEncounter(DateTime Time, string Attacker, string Victim);
        void AddCombatAction(int SwingType, bool Critical, string Special, string Attacker, string theAttackType, Advanced_Combat_Tracker.Dnum Damage, DateTime Time, int TimeSorter, string Victim, string theDamageType);
        int GlobalTimeSorter { get; set; }
    }

    public class ACTWrapper : IACTWrapper
    {
        public int GlobalTimeSorter
        {
            get
            {
                return Advanced_Combat_Tracker.ActGlobals.oFormActMain.GlobalTimeSorter;
            }

            set
            {
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.GlobalTimeSorter = value;
            }
        }

        public void AddCombatAction(int SwingType, bool Critical, string Special, string Attacker, string theAttackType, Dnum Damage, DateTime Time, int TimeSorter, string Victim, string theDamageType)
        {
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.AddCombatAction(SwingType, Critical, Special, Attacker, theAttackType, Damage, Time, TimeSorter, Victim, theDamageType);
        }

        public bool SetEncounter(DateTime Time, string Attacker, string Victim)
        {
            return Advanced_Combat_Tracker.ActGlobals.oFormActMain.SetEncounter(Time, Attacker, Victim);
        }
    }
}
