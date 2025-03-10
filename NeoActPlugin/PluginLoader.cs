using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeoActPlugin.Common;
using NeoActPlugin.Updater;
using NeoActPlugin.Core;

namespace NeoActPlugin
{
    public class PluginLoader : IActPluginV1
    {
        PluginMain pluginMain;
        Logger logger;
        static AssemblyResolver asmResolver;
        string pluginDirectory;
        TabPage pluginScreenSpace;
        Label pluginStatusText;
        bool initFailed = false;

        public TinyIoCContainer Container { get; private set; }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginDirectory = GetPluginDirectory();

            if (asmResolver == null)
            {
                asmResolver = new AssemblyResolver(new List<string>
                {
                    Path.Combine(pluginDirectory, "libs"),
                    Path.Combine(pluginDirectory, "addons"),
#if DEBUG
                    Path.Combine(pluginDirectory, "libs", Environment.Is64BitProcess ? "x64" : "x86"),
#else
                    //GetCefPath()
#endif
                });
            }

            this.pluginScreenSpace = pluginScreenSpace;
            this.pluginStatusText = pluginStatusText;

            if (!SanityChecker.LoadSaneAssembly("NeoActPlugin.Common") || !SanityChecker.LoadSaneAssembly("NeoActPlugin.Core") ||
                !SanityChecker.LoadSaneAssembly("NeoActPlugin.Updater"))
            {
                pluginStatusText.Text = Resources.FailedToLoadCommon;
                return;
            }

            Initialize();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private async void Initialize()
        {
            pluginStatusText.Text = Resources.InitRuntime;

            var container = new TinyIoCContainer();
            logger = new Logger();
            container.Register(logger);
            container.Register<ILogger>(logger);

            asmResolver.ExceptionOccured += (o, e) => logger.Log(LogLevel.Error, Resources.AssemblyResolverError, e.Exception);
            asmResolver.AssemblyLoaded += (o, e) => logger.Log(LogLevel.Info, Resources.AssemblyResolverLoaded, e.LoadedAssembly.FullName);

            this.Container = container;
            pluginMain = new PluginMain(pluginDirectory, logger, container);
            container.Register(pluginMain);

            pluginStatusText.Text = Resources.InitCef;

            SanityChecker.CheckDependencyVersions(logger);

            try
            {
                CurlWrapper.Init(pluginDirectory);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                ActGlobals.oFormActMain.WriteDebugLog(ex.ToString());
            }

            await FinishInit(container);
        }

        public async Task FinishInit(TinyIoCContainer container)
        {
            // wip
        }

        public void DeInitPlugin()
        {
            if (pluginMain != null && !initFailed)
            {
                pluginMain.DeInitPlugin();
            }

        }

        private string GetPluginDirectory()
        {
            var plugin = ActGlobals.oFormActMain.ActPlugins.Where(x => x.pluginObj == this).FirstOrDefault();
            if (plugin != null)
            {
                return Path.GetDirectoryName(plugin.pluginFile.FullName);
            }
            else
            {
                throw new Exception("Could not find ourselves in the plugin list!");
            }
        }
    }
}
