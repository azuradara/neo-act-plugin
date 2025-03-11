using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NeoActPlugin.Core
{
    interface IEventReceiver
    {
        string Name { get; }

        void HandleEvent(JObject e);
    }
}
