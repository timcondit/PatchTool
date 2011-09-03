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

            // Registry keys for all Envision apps that are eligible for updating via PacMan/Clyde are rooted at
            // SOFTWARE\Envision\Click2Coach.  I'm keeping the paths in the list to make it clear that these are keys,
            // as well as names.
            IEnumerable<string> patchableApps = new List<string> { "Server", "ChannelManager", "WMWrapperService", "Tools" };
            IEnumerator<string> pApps = patchableApps.GetEnumerator();

            // installedApps is the list of applications that are found in the registry on the target system
            IDictionary<string, string> installedApps = new Dictionary<string, string>();
            //RegistryKey rk = Registry.LocalMachine;

            while (pApps.MoveNext())
            {
                // DEBUG
                //Console.WriteLine(pApps.Current);
                //string installPath = @"SOFTWARE\Envision\Click2Coach\" + pApps.Current + @"\InstallPath";
                //string subKey = @"SOFTWARE/Envision/Click2Coach/" + pApps.Current;
                string subKey = @"SOFTWARE\Envision\Click2Coach\" + pApps.Current;
                //string installPath = @"SOFTWARE/Envision/Click2Coach/" + pApps.Current + @"/InstallPath";
                //Console.WriteLine("installPath: " + installPath);
                try
                {
                    //logger.Info("Looking for " + pApps.Current + " in the registry");
                    //RegistryKey rk = Registry.LocalMachine.OpenSubKey(installPath);
                    //Console.WriteLine("installPath: " + rk.GetValue(installPath));
                    Console.WriteLine("trying to open " + subKey);
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(subKey);
                    Console.WriteLine("rk:" + rk);
                    Console.WriteLine("trying to fetch InstallPath from " + subKey);
                    string regVal = Registry.GetValue(rk.ToString(), "InstallPath", "null").ToString();
                    //Registry.GetValue(rk.ToString(), "InstallPath", "null");
                    Console.WriteLine(regVal);
                    //Console.WriteLine("installPath: " + rk.GetValue(installPath));
                    //rk.OpenSubKey(installPath);
                    //rk.GetValue(regVal);
                    installedApps.Add(pApps.Current, regVal);
                    logger.Info(pApps.Current + " found in the registry at " + regVal);
                    rk.Close();
                }
                catch (NullReferenceException exc)
                {
                    logger.Info(pApps.Current + " not found in the registry at " + subKey);
                    //logger.Info(pApps.Current + " not found in the registry at " + installPath);
                    //logger.Info(exc.StackTrace);
                    //// ya right
                    //Console.WriteLine(exc.StackTrace);
                }
                catch (ArgumentException exc)
                {
                    logger.Info(pApps.Current + " not found in the registry at " + subKey);
                    //logger.Info(pApps.Current + " not found in the registry at " + installPath);
                    //logger.Info(exc.StackTrace);
                    //// ya right
                    //Console.WriteLine(exc.StackTrace);
                }
            }

            if (installedApps.Count == 0)
            {
                logger.Warn("No Envision applications were found on this machine!");
            }

            // When we get here, installedApps should have at least one key-value pair representing an application.
            // If it's empty, there's nothing installed.
            logger.Info("Found " + installedApps.Count + " Envision applications:");
            foreach (KeyValuePair<string, string> item in installedApps)
            {
                logger.Info(item.Key);
            }

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
            Environment.Exit(0);

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
