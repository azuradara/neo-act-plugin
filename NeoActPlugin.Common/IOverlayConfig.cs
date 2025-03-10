using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoActPlugin.Common
{
    public interface IOverlayConfig
    {
        string Name { get; set; }
        bool IsVisible { get; set; }
        bool IsClickThru { get; set; }
        bool HideOutOfCombat { get; set; }
        Point Position { get; set; }
        Size Size { get; set; }
        string Url { get; set; }
        int MaxFrameRate { get; set; }
        bool GlobalHotkeyEnabled { get; set; }
        Keys GlobalHotkey { get; set; }
        Keys GlobalHotkeyModifiers { get; set; }

        Type OverlayType { get; }
    }
}
