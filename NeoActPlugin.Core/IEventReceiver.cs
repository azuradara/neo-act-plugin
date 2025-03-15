using Newtonsoft.Json.Linq;

namespace NeoActPlugin.Core
{
    interface IEventReceiver
    {
        string Name { get; }

        void HandleEvent(JObject e);
    }
}
