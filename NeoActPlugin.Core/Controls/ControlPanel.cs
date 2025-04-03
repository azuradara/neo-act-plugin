using NeoActPlugin.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NeoActPlugin.Core
{
    public partial class ControlPanel : System.Windows.Forms.UserControl
    {
        TinyIoCContainer _container;
        ILogger _logger;
        PluginMain _pluginMain;
        TabPage _generalTab, _eventTab;
        bool logResized = false;
        bool logConnected = false;

        static Dictionary<string, string> esNames = new Dictionary<string, string>
        {
            { "MiniParseEventSource", Resources.MapESMiniParse },
        };
        static Dictionary<string, string> overlayNames = new Dictionary<string, string>
        {
            { "LabelOverlay", Resources.MapOverlayLabel },
            { "MiniParseOverlay", Resources.MapOverlayMiniParse },
            { "SpellTimerOverlay", Resources.MapOverlaySpellTimer },
        };

        public ControlPanel(TinyIoCContainer container)
        {
            InitializeComponent();

            _container = container;
            _logger = container.Resolve<ILogger>();
            _pluginMain = container.Resolve<PluginMain>();

            this.checkBoxFollowLog.Checked = true;

            _generalTab = new ConfigTabPage
            {
                Name = Resources.GeneralTab,
                Text = "",
            };

            _eventTab = new ConfigTabPage
            {
                Name = Resources.EventConfigTab,
                Text = "",
            };

            logBox.Text = Resources.LogNotConnectedError;
            _logger.RegisterListener(AddLogEntry);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                _logger.ClearListener();
            }

            base.Dispose(disposing);
        }

        private void AddLogEntry(LogEntry entry)
        {
            var msg = $"[{entry.Time}] {entry.Level}: {entry.Message}" + Environment.NewLine;

            if (!logConnected)
            {
                // Remove the error message about the log not being connected since it is now.
                logConnected = true;
                logBox.Text = "";
            }
            else if (logBox.TextLength > 200 * 1024)
            {
                logBox.Text = "============ LOG TRUNCATED ==============\nThe log was truncated to reduce memory usage.\n=========================================\n" + msg;
                return;
            }

            if (checkBoxFollowLog.Checked)
            {
                logBox.AppendText(msg);
            }
            else
            {
                // This is based on https://stackoverflow.com/q/1743448
                bool bottomFlag = false;
                int sbOffset;
                int savedVpos;

                // Win32 magic to keep the textbox scrolling to the newest append to the textbox unless
                // the user has moved the scrollbox up
                sbOffset = (int)((logBox.ClientSize.Height - SystemInformation.HorizontalScrollBarHeight) / (logBox.Font.Height));
                savedVpos = NativeMethods.GetScrollPos(logBox.Handle, NativeMethods.SB_VERT);
                NativeMethods.GetScrollRange(logBox.Handle, NativeMethods.SB_VERT, out _, out int VSmax);

                if (savedVpos >= (VSmax - sbOffset - 1))
                    bottomFlag = true;

                logBox.AppendText(msg);

                if (bottomFlag)
                {
                    NativeMethods.GetScrollRange(logBox.Handle, NativeMethods.SB_VERT, out _, out VSmax);
                    savedVpos = VSmax - sbOffset;
                }
                NativeMethods.SetScrollPos(logBox.Handle, NativeMethods.SB_VERT, savedVpos, true);
                NativeMethods.PostMessageA(logBox.Handle, NativeMethods.WM_VSCROLL, NativeMethods.SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
            }
        }

        private void ButtonClearLog_Click(object sender, EventArgs e)
        {
            logBox.Clear();
        }

        private void RegionPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Reader._region = regionPicker.SelectedItem.ToString();
            PluginMain.WriteLog(LogLevel.Info, "Selected Region: " + regionPicker.SelectedItem.ToString());
            Reader _ = new Reader();
            _.RefreshPointers();
        }

        private class ConfigTabPage : TabPage
        {
            public bool IsOverlay = false;
            public bool IsEventSource = false;
        }

        private void checkBoxFollowLog_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
