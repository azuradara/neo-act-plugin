using NeoActPlugin.Common;
using System;
using System.Windows.Forms;

namespace NeoActPlugin.Core.Controls
{
    public partial class LogPanel : UserControl
    {
        public LogPanel(TinyIoCContainer container)
        {
            InitializeComponent();

            container.Resolve<ILogger>().RegisterListener((entry) =>
            {
                logBox.AppendText($"[{entry.Time}] {entry.Level}: {entry.Message}" + Environment.NewLine);
            });
        }
    }
}