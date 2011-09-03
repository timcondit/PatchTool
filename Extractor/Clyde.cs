using CommandLine;
using CommandLine.Text;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

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
            // TC: Clyde needs no arguments.  APPDIR is read from the registry; patchVersion is set
            // when the patch is created; and extractDir is composed from all three.
            //
            // Late note: this is about to be not true.  Clyde needs the patch name (in the form of
            // APPNAME-PATCHVER.exe).
            Extractor e = new Extractor();

            IEnumerable<string> patchableApps = new List<string> { "Server", "ChannelManager", "WMWrapperService", "Tools" };
            IEnumerator<string> pApps = patchableApps.GetEnumerator();
            IDictionary<string, string> installedApps = new Dictionary<string, string>();

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
                    logger.Info("InstallPath found for " + pApps.Current);
                    rk.Close();
                }
                catch (NullReferenceException)
                {
                    logger.Info("InstallPath not found for " + pApps.Current);
                }
                catch (ArgumentException)
                {
                    logger.Info("InstallPath not found for " + pApps.Current);
                }
            }

            if (installedApps.Count == 0)
            {
                logger.Warn("No Envision applications were found on this machine!");
            }
            else
            {
                logger.Info("Found " + installedApps.Count + " Envision applications:");
                foreach (KeyValuePair<string, string> item in installedApps)
                {
                    string msg = item.Key + " installed at [" + item.Value + "]";
                    logger.Info(msg);
                }
            }

            // TC: read the APPDIR from the registry
            RegistryKey hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"SOFTWARE\Envision\Click2Coach\ChannelManager");

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
                e.run(Path.Combine(e.ExtractDir, "ROOT"), e.AppDir);
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

        //private static string PathName(string _name)
        //{
        //    get
        //    {
        //        RegistryKey rk = Registry.LocalMachine;
        //        rk.OpenSubKey(@"SOFTWARE\Envision\Click2Coach\ChannelManager");
        //        return (string)rk.GetValue("InstallPath");
        //    }
        //}
    }
}
