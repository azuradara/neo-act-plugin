﻿using NeoActPlugin.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace NeoActPlugin
{
    public static class SanityChecker
    {
        public static bool LoadSaneAssembly(string name)
        {
            var loaderVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Assembly asm = null;
            Version asmVersion = null;

            try
            {
                asm = Assembly.Load(name);
                asmVersion = asm.GetName().Version;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Resources.AssemblyMissing, name, ex),
                    "OverlayPlugin Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }

            if (loaderVersion != asmVersion)
            {
                MessageBox.Show(
                    string.Format(Resources.AssemblyMismatch, asm.Location, asmVersion, loaderVersion),
                    "OverlayPlugin Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }

            return true;
        }

        public static void CheckDependencyVersions(ILogger logger)
        {
            var expectedVersions = new Dictionary<string, string>
            {
                { "Newtonsoft.Json", "12.0.0" },
            };


            foreach (var pair in expectedVersions)
            {
                Version asmVersion = null;

                try
                {
                    var asm = Assembly.Load(pair.Key);
                    asmVersion = asm.GetName().Version;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Resources.DependencyMissing, pair.Key, ex),
                        "OverlayPlugin Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }

                if (asmVersion != null && asmVersion < Version.Parse(pair.Value))
                {
                    logger.Log(LogLevel.Error, string.Format(Resources.DependencyOutdated, pair.Key, asmVersion, pair.Value));
                }
            }
        }
    }
}
