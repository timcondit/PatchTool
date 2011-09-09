using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using Microsoft.Win32;
using Nini.Config;
using NLog;

namespace PatchTool
{
    class Clyde
    {
        private sealed class Options
        {
            #region Standard Option Attribute
            [Option("r", "patchVersion",
                    Required = true,
                    HelpText = "The version number for this patch.")]
            public string patchVersion = String.Empty;

            [HelpOption(
                    HelpText = "Display this help screen.")]

            public string GetUsage()
            {
                var help = new HelpText("Envision Package Manager");
                help.AdditionalNewLineAfterOption = true;
                help.Copyright = new CopyrightInfo("Envision Telephony, Inc.", 2011);
                help.AddPreOptionsLine("Usage: Clyde -r<patchVersion>");
                help.AddPreOptionsLine("       Clyde -?");
                help.AddOptions(this);

                return help;
            }
            #endregion
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // Get the intersection of those applications which are patched with those which are installed.  For
            // example, if Server, ChannelManager and Tools are patched, but only Server and ChannelManager are
            // installed, then we don't patch Tools.  But it may be staged if it's easier to do it than not.
            //
            // getInstalledApps() returns a dictionary { appName => registryPath } for all installed apps.  How to get
            // the patchedApps?

            // get a dictionary of applications installed on the target machine (key:appName, value:APPDIR)
            IEnumerable<string> patchableApps = new List<string> { "Server", "ChannelManager", "WMWrapperService", "Tools" };
            IDictionary<string, string> installedApps = getInstalledApps(patchableApps);

            //// Get list of applications to be patched straight from the archive.  This feels a little brittle, but
            //// maybe it's not bad.
            //IEnumerable<string> appsToPatch = new List<string> { "Server", "Tools" };

            // Update the values in <patchVersion>.manifest with registry keys for installed apps.  If 

            // TC: read the APPDIR from the registry
            RegistryKey hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"SOFTWARE\Envision\Click2Coach\ChannelManager");
            Console.WriteLine("hklm: {0}", hklm);

            Extractor e = new Extractor();
            e.AppDir = hklm.GetValue("InstallPath", "rootin tootin").ToString();

            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            if (options.patchVersion == String.Empty)
            {
                // "pretty it up" and exit
                throw new ArgumentException("something's broken! (options.patchVersion)");
            }
            else
            {
                e.PatchVersion = options.patchVersion;
            }

            // beware System.IO.DirectoryNotFoundException
            //
            // NB: may need "C:\patches\d7699dbd-8214-458e-adb0-8317dfbfaab1>runas /env /user:administrator Clyde.exe"
            try
            {
                // TC: few things TODO
                // 1: add a Console title (somewhere, maybe not here)
                // 2:: tell the user what we're doing here (pre-file-move check)
                // 3: add simple continue or cancel here?
                // 4: get rid of "ROOT" - should be "e.run(e.ExtractDir, e.AppDir);"

                // TC: for testing
                //Console.Write("Press any key to continue");
                //Console.ReadLine();

                e.run(Path.Combine(e.ExtractDir, "files"), e.AppDir);
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Clyde must be run as Administrator on this system", "sorry Charlie");
                throw;
            }

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }

        private static IDictionary<string, string> getInstalledApps(IEnumerable<string> patchableApps)
        {
            // I may ditch this installedApps and write these values out to patch.manifest instead
            IDictionary<string, string> installedApps = new Dictionary<string, string>();
            IEnumerator<string> pApps = patchableApps.GetEnumerator();
            // swap out NULL values for paths to installed apps
            IniConfigSource installedApps2 = new IniConfigSource("patch.manifest");
            IConfig appsToPatch = installedApps2.Configs["AppsToPatch"];

            while (pApps.MoveNext())
            {
                try
                {
                    // Create a new RegistryKey instance every time, or the value detection fails (don't know why)
                    string subKey = @"SOFTWARE\Envision\Click2Coach\" + pApps.Current;
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(subKey);

                    // Registry.GetValue() throws an ArgumentException if the value is not found.  It's an error if the
                    // "null" is passed to installedApps, but the method requires a default value.
                    string installPath = Registry.GetValue(rk.ToString(), "InstallPath", "null").ToString();
                    installedApps.Add(pApps.Current, installPath);
                    appsToPatch.Set(pApps.Current, installPath);

                    logger.Info("InstallPath found for {0}", pApps.Current);
                    rk.Close();
                }
                catch (NullReferenceException)
                {
                    logger.Info("InstallPath not found for {0}", pApps.Current);
                }
                catch (ArgumentException)
                {
                    logger.Info("InstallPath not found for {0}", pApps.Current);
                }
            }
            installedApps2.Save("patch.manifest");

            if (installedApps.Count == 0)
            {
                logger.Warn("No Envision applications were found on this machine!");
            }
            else
            {
                logger.Info("Found {0} Envision applications:", installedApps.Count);
                foreach (KeyValuePair<string, string> item in installedApps)
                {
                    logger.Info("{0} installed at \"{1}\"", item.Key, item.Value);
                }
            }
            return installedApps;
        }

        // Can/should I just search for the appsToPatch by name in the root of the just-extracted archive?
        //
        // Maybe better: write an appsToPatch.config in PacMan, and include it in Clyde.  For now it will have one
        // section, which lists the apps to patch.  Later on it should include the patch contents for each app, and
        // replace the appKeys Lists in PacMan.
        private static IEnumerable<string> appsToPatch()
        {
            throw new NotImplementedException();
        }
    }
}
