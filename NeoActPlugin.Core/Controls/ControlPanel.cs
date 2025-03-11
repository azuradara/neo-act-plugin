using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using Microsoft.Win32;
using NeoActPlugin.Common;
using System.Diagnostics.Tracing;
using System.Web.UI;

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
            tableLayoutPanel0.PerformLayout();
            // Make the log box big until we load the overlays since the log is going to be *very*
            // important if we never make it that far.
            splitContainer1.SplitterDistance = 5;

            _container = container;
            _logger = container.Resolve<ILogger>();
            _pluginMain = container.Resolve<PluginMain>();

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

            Resize += (o, e) =>
            {
                if (!logResized && Height > 500 && tabControl.TabCount > 0)
                {
                    ResizeLog();
                }
            };
        }

        public void ResizeLog()
        {
            if (!logResized)
            {
                // Only make this the final resize if we have enough height to make this layout usable.
                logResized = Height > 500;

                // Overlay tabs have been initialised, everything is fine; make the log small again.
                splitContainer1.SplitterDistance = (int)Math.Round(Height * 0.75);
            }
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

        private class ConfigTabPage : TabPage
        {
            public bool IsOverlay = false;
            public bool IsEventSource = false;
        }
    }
}
