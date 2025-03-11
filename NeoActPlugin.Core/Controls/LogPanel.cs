using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeoActPlugin.Common;

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