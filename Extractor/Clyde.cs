using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using Microsoft.Win32;
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
            Extractor e = new Extractor();

            // Get the intersection of those applications which are patched with those which are installed.  For
            // example, if Server, ChannelManager and Tools are patched, but only Server and ChannelManager are
            // installed, then we don't patch Tools.  But it may be staged if it's easier to do it than not.

            IDictionary<string, string> patchableApps = new Dictionary<string, string>();
            patchableApps.Add("Server", "Envision Server Suite");
            patchableApps.Add("ChannelManager", "Envision Channel Manager");
            patchableApps.Add("WebApps", "Envision Web Apps");
            patchableApps.Add("WMWrapperService", "Envision Windows Media Wrapper Service");
            patchableApps.Add("Tools", "Envision Tools Suite");

            // first check
            IDictionary<string, string> installedApps = e.getInstalledApps(patchableApps.Keys);

            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            e.PatchVersion = options.patchVersion;

            foreach (string iApp in installedApps.Keys)
            {
                try
                {
                    string appDir = installedApps[iApp];
                    string srcDirRoot = Path.Combine(e.ExtractDir, e.SourceDir);
                    e.run(Path.Combine(srcDirRoot, iApp), appDir);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Clyde must be run as Administrator on this system", "sorry Charlie");
                    throw;
                }

                // TC: for testing
                Console.Write("Press any key to continue");
                Console.ReadLine();
            }

            // second check comes much later (partly redundant if done right, which it's not at the moment)
            //
            // TODO: given the value from patchableApps, how to get the key, and use it to update installedApps?
            // In other words, from "Envision Web Apps" installedApps.Add("WebApps", wheresWebApps).
            string wheresWebApps = e.GetInstallLocation("Envision Web Apps");
            if (wheresWebApps != "NONE")
            {
                try
                {
                    string srcDirRoot = Path.Combine(e.ExtractDir, e.SourceDir);
                    e.run(Path.Combine(srcDirRoot, "WebApps"), wheresWebApps, true);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Clyde must be run as Administrator on this system", "sorry Charlie");
                    throw;
                }
            }
            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
