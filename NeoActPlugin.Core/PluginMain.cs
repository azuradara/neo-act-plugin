using Advanced_Combat_Tracker;
using NeoActPlugin.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoActPlugin.Core
{
    public class PluginMain : UserControl, Advanced_Combat_Tracker.IActPluginV1
    {
        #region Designer Created Code (Avoid editing)
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            lstMessages = new System.Windows.Forms.ListBox();
            this.cmdClearMessages = new System.Windows.Forms.Button();
            this.cmdCopyProblematic = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 82;
            this.label1.Text = "Parser Messages";
            //
            // lstMessages
            //
            lstMessages.FormattingEnabled = true;
            lstMessages.Location = new System.Drawing.Point(14, 41);
            lstMessages.Name = "lstMessages";
            lstMessages.ScrollAlwaysVisible = true;
            lstMessages.Size = new System.Drawing.Size(700, 264);
            lstMessages.TabIndex = 81;
            //
            // cmdClearMessages
            //
            this.cmdClearMessages.Location = new System.Drawing.Point(88, 311);
            this.cmdClearMessages.Name = "cmdClearMessages";
            this.cmdClearMessages.Size = new System.Drawing.Size(106, 26);
            this.cmdClearMessages.TabIndex = 84;
            this.cmdClearMessages.Text = "Clear";
            this.cmdClearMessages.UseVisualStyleBackColor = true;
            this.cmdClearMessages.Click += new System.EventHandler(this.cmdClearMessages_Click);
            this.cmdCopyProblematic.Location = new System.Drawing.Point(478, 311);
            this.cmdCopyProblematic.Name = "cmdCopyProblematic";
            this.cmdCopyProblematic.Size = new System.Drawing.Size(118, 26);
            this.cmdCopyProblematic.TabIndex = 85;
            this.cmdCopyProblematic.Text = "Copy to Clipboard";
            this.cmdCopyProblematic.UseVisualStyleBackColor = true;
            this.cmdCopyProblematic.Click += new System.EventHandler(this.cmdCopyProblematic_Click);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdCopyProblematic);
            this.Controls.Add(this.cmdClearMessages);
            this.Controls.Add(this.label1);
            this.Controls.Add(lstMessages);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(728, 356);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        private static System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.Button cmdClearMessages;
        private System.Windows.Forms.Button cmdCopyProblematic;

        #endregion Designer Created Code (Avoid editing)

        private TinyIoCContainer _container;
        private ILogger _logger;

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

            InitializeComponent();
        }

        private System.Windows.Forms.Label lblStatus = null;

        public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
        {
            lblStatus = pluginStatusText;

            if (!IsRunningAsAdmin())
            {
                lblStatus.Text = "Error: Run ACT as Administrator.";

                MessageBox.Show(
                    "NeoActPlugin requires ACT to be run as Administrator. Please restart ACT with elevated privileges.",
                    "Admin Rights Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                this.DeInitPlugin();

                return;
            }

            try
            {
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.UpdateCheckClicked += new Advanced_Combat_Tracker.FormActMain.NullDelegate(UpdateCheckClicked);
                if (Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                {
                    Thread updateThread = new Thread(new ThreadStart(UpdateCheckClicked));
                    updateThread.IsBackground = true;
                    updateThread.Start();
                }

                UpdateACTTables();

                LogParse.Initialize(new ACTWrapper());

                pluginScreenSpace.Controls.Add(this);
                this.Dock = DockStyle.Fill;

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogPathHasCharName = false;
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogFileFilter = "*.log";

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.TimeStampLen = DateTime.Now.ToString("HH:mm:ss.fff").Length + 1;

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetDateTimeFromLog = new FormActMain.DateTimeLogParser(LogParse.ParseLogDateTime);

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(LogParse.BeforeLogLineRead);

                Advanced_Combat_Tracker.ActGlobals.oFormActMain.ChangeZone("Blade & Soul");

                LogWriter.Initialize();

                lblStatus.Text = "BnS Plugin Started.";
            }
            catch (Exception ex)
            {
                LogParserMessage("Exception during InitPlugin: " + ex.ToString().Replace(Environment.NewLine, " "));
                lblStatus.Text = "InitPlugin Error.";
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
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead -= LogParse.BeforeLogLineRead;

            if (lblStatus != null)
            {
                lblStatus.Text = "BnS Plugin Unloaded.";
                lblStatus = null;
            }
        }


        public void UpdateCheckClicked()
        {

        }

        private void UpdateACTTables()
        {

        }


        public static void LogParserMessage(string message)
        {
            if (lstMessages != null && !lstMessages.IsDisposed)
                lstMessages.Invoke(new Action(() => lstMessages.Items.Add(message)));
        }

        private void cmdClearMessages_Click(object sender, EventArgs e)
        {
            lstMessages.Items.Clear();
        }

        private void cmdCopyProblematic_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object itm in lstMessages.Items)
                sb.AppendLine((itm ?? "").ToString());

            if (sb.Length > 0)
                System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }
    }

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
                PluginMain.LogParserMessage("Error [BNS_Log.Uninitialize] " + ex.ToString().Replace(Environment.NewLine, " "));
            }
        }

        private static void WorkerMain()
        {
            while (!_stopRequested)
            {
                try
                {
                    var reader = new Reader();
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
                PluginMain.LogParserMessage(errorMessage);
            }
            catch { /* Prevent logging failures from crashing thread */ }
        }
    }

    public static class LogParse
    {
        public static Regex regex_incomingdamage1 = new Regex(@"(?<target>.+?)?( received|Received) (?<damage>\d+(,\d+)*) ((?<critical>Critical) )?damage((,)?( and)? (?<HPDrain>\d+(,\d+)*) HP drain)?((,)?( and)? (?<FocusDrain>\d+) Focus drain)?((,)?( and)? (?<debuff>.+?))? from ((?<actor>.+?)&apos;s )?(?<skill>.+?)((,)?( but)? resisted (?<resistdebuff>.+?)( effect)?)?\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regex_incomingdamage2 = new Regex(@"((?<target>.+?) )?(Blocked|blocked|partially blocked|countered)( (?<actor>.+)&apos;s)? (?<skill>.+?) (but received|receiving)( (?<damage>\d+(,\d+)*) damage)?(( and)? (?<HPDrain>\d+(,\d+)*) HP drain)?( and?)?( (?<debuff>.+?))?\.", RegexOptions.Compiled);
        public static Regex regex_incomingdamage3 = new Regex(@"(?<actor>.+?)&apos;s (?<skill>.+?) inflicted( (?<damage>\d+(,\d+)*) damage)?( and)?( (?<debuff>.+?))*?( to (?<target>.+?))?\.", RegexOptions.Compiled);
        public static Regex regex_yourdamage = new Regex(@"(?<skill>.+?)\s+(?<critical>(critically hit)|(hit))\s+(?<target>.+?)\s+for\s+(?<damage>\d+(,\d+)*)\s+damage(((, draining| and drained)\s+((?<HPDrain>\d+(,\d+)*)\s+HP)?(\s+and\s+)?((?<FocusDrain>\d+)\s+Focus)?))?(,\s+removing\s+(?<skillremove>.+?))?\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regex_debuff2 = new Regex(@"((?<actor>.+?)&apos;s )?(?<skill>.+?)( (?<critical>(critically hit)|(hit)) (?<target>.+?))? ((and )?inflicted (?<debuff>.+?))?(but (?<debuff2>.+?) was resisted)?\.", RegexOptions.Compiled);
        public static Regex regex_evade = new Regex(@"(?<target>.+?) evaded (?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_defeat = new Regex(@"(?<target>.+?) (was|were) (defeated|rendered near death|killed) by ((?<actor>.+?)&apos;s )?(?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_debuff = new Regex(@"(?<target>.+?) (receives|resisted) (?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_heal = new Regex(@"(?<target>.+?)?( recovered|Recovered) ((?<HPAmount>\d+(,\d+)*) HP)?((?<FocusAmount>\d+) Focus)? (with|from) (?<skill>.+?)\.");
        public static Regex regex_buff = new Regex(@"(?<skill>.+?) is now active\.", RegexOptions.Compiled);

        private static IACTWrapper _ACT = null;

        public static void Initialize(IACTWrapper ACT)
        {
            _ACT = ACT;
        }

        public static DateTime ParseLogDateTime(string message)
        {
            DateTime ret = DateTime.MinValue;

            if (_ACT == null)
                throw new ApplicationException("ACT Wrapper not initialized.");

            try
            {
                if (message == null) return ret;

                if (message.Contains("|"))
                {
                    int pipeIndex = message.IndexOf('|');
                    string timestampPart = message.Substring(0, pipeIndex);
                    if (!DateTime.TryParse(timestampPart, out ret))
                    {

                        PluginMain.LogParserMessage("Failed to parse timestamp");
                        return DateTime.MinValue;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginMain.LogParserMessage("Error [ParseLogDateTime] " + ex.ToString().Replace(Environment.NewLine, " "));
            }
            return ret;
        }

        public static void BeforeLogLineRead(bool isImport, Advanced_Combat_Tracker.LogLineEventArgs logInfo)
        {
            string logLine = logInfo.logLine;

            if (_ACT == null)
                throw new ApplicationException("ACT Wrapper not initialized.");

            try
            {
                DateTime timestamp = ParseLogDateTime(logLine);
                if (logLine.Contains("|"))
                {
                    int pipeIndex = logLine.IndexOf('|');
                    logLine = logLine.Substring(pipeIndex + 1);
                }

                logInfo.logLine = string.Format("[{0:HH:mm:ss.fff}] {1}", timestamp, logLine);

                Match m;

                m = regex_yourdamage.Match(logLine);
                if (m.Success)
                {
                    string actor = "You";
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    string damage = (m.Groups["damage"].Value ?? "").Replace(",", "");
                    string hpdrain = (m.Groups["HPDrain"].Value ?? "").Replace(",", "");

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            m.Groups["critical"].Value == "critically hit",
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            new Advanced_Combat_Tracker.Dnum(int.Parse(damage)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                        if (m.Groups["HPDrain"].Success)
                        {
                            _ACT.AddCombatAction(
                                (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                                false,
                                "Drain",
                                actor,
                                DecodeString(m.Groups["skill"].Value),
                                new Advanced_Combat_Tracker.Dnum(int.Parse(hpdrain)),
                                timestamp,
                                _ACT.GlobalTimeSorter,
                                actor,
                                "");
                        }

                    }

                    return;
                }

                m = regex_incomingdamage1.Match(logLine);
                if (!m.Success)
                    m = regex_incomingdamage2.Match(logLine);
                if (!m.Success)
                    m = regex_incomingdamage3.Match(logLine);
                if (m.Success)
                {
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    string skill = m.Groups["skill"].Success ? DecodeString(m.Groups["skill"].Value) : "";
                    string damage = (m.Groups["damage"].Value ?? "").Replace(",", "");
                    string hpdrain = (m.Groups["HPDrain"].Value ?? "").Replace(",", "");

                    // if skillname is blank, the skillname and actor may be transposed
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        if (!string.IsNullOrWhiteSpace(actor))
                        {
                            // "Received 1373 damage from Rising Blaze&apos;s ."
                            skill = actor;
                        }
                    }

                    // Fix for "Received 1373 damage from Balefire&apos;s Bleed
                    string[] invalidSkills = { "Hellfire", "Venom", "Lasting Effects", "Bleed", "Poison", "Venom Swarm", "Explosive Rage&apos;s Poison", "Flame Breath&apos;s Lasting Effects" };

                    if (!string.IsNullOrWhiteSpace(actor) && Array.Exists(invalidSkills, e => e == skill))
                    {
                        // using the actor rather than the skill allows users to
                        // recognize their skills by checking Unknown's skill breakdown.
                        // the damage lost here should be negligible in the grand scheme of things

                        skill = actor;
                        actor = "Unknown";
                    }

                    if (string.IsNullOrWhiteSpace(target))
                        target = "You";

                    if (string.IsNullOrWhiteSpace(actor))
                        actor = "Unknown";

                    // todo: in the future, if damage is missing, still parse the buff portion
                    if (!m.Groups["damage"].Success)
                        return;
                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            m.Groups["critical"].Value == "Critical",
                            "",
                            actor,
                            skill,
                            new Advanced_Combat_Tracker.Dnum(int.Parse(damage)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                        if (m.Groups["HPDrain"].Success)
                        {
                            _ACT.AddCombatAction(
                                (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                                false,
                                "Drain",
                                actor,
                                skill,
                                new Advanced_Combat_Tracker.Dnum(int.Parse(hpdrain)),
                                timestamp,
                                _ACT.GlobalTimeSorter,
                                actor,
                                "");
                        }
                    }

                    return;
                }

                m = regex_heal.Match(logLine);
                if (m.Success)
                {
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    if (string.IsNullOrWhiteSpace(target))
                        target = "You";
                    string actor = "Unknown";

                    // do not process if there is no HP amount.
                    if (!m.Groups["HPAmount"].Success)
                        return;

                    string hpamount = (m.Groups["HPAmount"].Value ?? "").Replace(",", "");

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                            false,
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            new Advanced_Combat_Tracker.Dnum(int.Parse(hpamount)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                    }
                    return;
                }


                m = regex_debuff2.Match(logLine);
                if (m.Success)
                {
                    // todo: add debuff support
                    return;
                }

                m = regex_debuff.Match(logLine);
                if (m.Success)
                {
                    // todo: add debuff support
                    return;
                }

                m = regex_buff.Match(logLine);
                if (m.Success)
                {
                    // todo: add buff support
                    return;
                }

                m = regex_evade.Match(logLine);
                if (m.Success)
                {
                    // todo: add evade support
                    return;
                }

                m = regex_defeat.Match(logLine);
                if (m.Success)
                {
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    if (string.IsNullOrWhiteSpace(actor))
                        actor = "Unknown";

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            false,
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            Advanced_Combat_Tracker.Dnum.Death,
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString().Replace(Environment.NewLine, " ");
                if (ex.InnerException != null)
                    exception += " " + ex.InnerException.ToString().Replace(Environment.NewLine, " ");

                PluginMain.LogParserMessage("Error [LogParse.BeforeLogLineRead] " + exception + " " + logInfo.logLine);
            }

            // For debugging
            if (!string.IsNullOrWhiteSpace(logLine))
                PluginMain.LogParserMessage("Unhandled Line: " + logInfo.logLine);
        }

        private static string DecodeString(string data)
        {
            string ret = data.Replace("&apos;", "'")
                .Replace("&amp;", "&");

            return ret;
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

    class Reader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumProcessModules(
            IntPtr hProcess,
            [Out] IntPtr[] lphModule,
            uint cb,
            out uint lpcbNeeded);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_VM_READ = 0x0010,
            PROCESS_QUERY_INFORMATION = 0x0400
        }

        private readonly int _pid;
        private IntPtr _baseAddress;
        private IntPtr _currentAddress;
        private int _offsetCounter = 1;
        private readonly long[] _offsets = { 0x07423C90, 0x490, 0x490, 0x670, 0x8, 0x70 };
        private DateTime _lastRefreshTime = DateTime.MinValue;

        // not sure what i should set this to, but i like 4
        private TimeSpan _refreshInterval = TimeSpan.FromSeconds(4);

        public Reader()
        {
            var pid = GetProcessId("BNSR.exe");
            if (!pid.HasValue)
                throw new ArgumentException("Process not found: " + "BNSR");

            _pid = pid.Value;
            RefreshPointers();
        }

        private void RefreshPointers()
        {
            try
            {
                _baseAddress = GetBaseAddress(_pid);
                _currentAddress = FollowPointerChain(_pid, _baseAddress, _offsets);

                if (_currentAddress == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to resolve pointer chain");

                int currentOffset = 0;
                while (true)
                {
                    var targetAddress = new IntPtr(_currentAddress.ToInt64() + (currentOffset * 0x70));
                    var pointerBuffer = ReadMemory(targetAddress, 8);
                    if (pointerBuffer == null || IsAllZero(pointerBuffer))
                        break;
                    currentOffset++;
                }

                _offsetCounter = currentOffset;
                _lastRefreshTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                PluginMain.LogParserMessage("Error refreshing pointers: " + ex.Message);
            }
        }

        public IEnumerable<string> Read()
        {
            if (DateTime.Now - _lastRefreshTime > _refreshInterval)
            {
                RefreshPointers();
            }

            int lastOffset = -1;

            while (true)
            {
                lastOffset = _offsetCounter;

                var targetAddress = new IntPtr(_currentAddress.ToInt64() + (_offsetCounter * 0x70));
                _offsetCounter++;

                var pointerBuffer = ReadMemory(targetAddress, 8);
                if (pointerBuffer == null) yield break;

                Thread.Sleep(1);

                while (IsAllZero(pointerBuffer))
                {
                    Thread.Sleep(100);

                    if (DateTime.Now - _lastRefreshTime > _refreshInterval)
                    {
                        RefreshPointers();
                        targetAddress = new IntPtr(_currentAddress.ToInt64() + ((_offsetCounter - 1) * 0x70));
                    }

                    pointerBuffer = ReadMemory(targetAddress, 8);
                    if (pointerBuffer == null) yield break;
                }

                var nextAddress = new IntPtr(BitConverter.ToInt64(pointerBuffer, 0));
                if (nextAddress == IntPtr.Zero) yield break;

                var stringBuffer = ReadMemory(nextAddress, 2048);
                if (stringBuffer == null) yield break;

                var decoded = DecodeString(stringBuffer);
                if (!string.IsNullOrEmpty(decoded) && _offsetCounter != lastOffset)
                    yield return decoded;
            }
        }

        private byte[] ReadMemory(IntPtr address, int size)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.PROCESS_VM_READ, false, _pid);
            if (processHandle == IntPtr.Zero)
                return null;

            try
            {
                var buffer = new byte[size];
                int bytesRead;
                bool success = ReadProcessMemory(processHandle, address, buffer, size, out bytesRead);

                return success && bytesRead == size ? buffer : null;
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        private static string DecodeString(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                if (buffer[i] == 0 && buffer[i + 1] == 0)
                    return Encoding.Unicode.GetString(buffer, 0, i);
            }
            return Encoding.Unicode.GetString(buffer);
        }

        private static bool IsAllZero(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                if (buffer[i] != 0) return false;
            return true;
        }

        private static int? GetProcessId(string processName)
        {
            var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : (int?)null;
        }

        private IntPtr GetBaseAddress(int pid)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_INFORMATION | ProcessAccessFlags.PROCESS_VM_READ, false, pid);
            if (hProcess == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            uint bytesNeeded;
            if (!EnumProcessModules(hProcess, null, 0, out bytesNeeded))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            IntPtr[] modules = new IntPtr[bytesNeeded / IntPtr.Size];
            if (!EnumProcessModules(hProcess, modules, bytesNeeded, out bytesNeeded))
            {
                CloseHandle(hProcess);
                return IntPtr.Zero;
            }

            CloseHandle(hProcess);
            return modules.Length > 0 ? modules[0] : IntPtr.Zero;
        }

        private IntPtr FollowPointerChain(int pid, IntPtr baseAddress, long[] offsets)
        {
            var currentAddress = baseAddress;
            for (int i = 0; i < offsets.Length; i++)
            {
                currentAddress = new IntPtr(currentAddress.ToInt64() + offsets[i]);
                if (i >= offsets.Length - 1) continue;

                var buffer = ReadMemory(currentAddress, 8);
                if (buffer == null) return IntPtr.Zero;
                currentAddress = new IntPtr(BitConverter.ToInt64(buffer, 0));
            }
            return currentAddress;
        }
    }
}
