using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;

namespace PatchTool
{
    class Clyde
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Extractor e = new Extractor();

            // Do I really need a Dictionary here?  So far I'm not seeing it.
            IDictionary<string, string> allInstallers = new Dictionary<string, string>();
            allInstallers.Add("Server", "Envision Server");
            allInstallers.Add("ServerSuite", "Envision Server Suite");
            allInstallers.Add("ChannelManager", "Envision Channel Manager");
            allInstallers.Add("Centricity", "Envision Centricity");
            allInstallers.Add("WebApps", "Envision Web Apps");
            allInstallers.Add("WMWrapperService", "Envision Windows Media Wrapper Service");
            allInstallers.Add("DBMigration", "Envision Database Migration");
            allInstallers.Add("Tools", "Envision Tools Suite");

            ApplicationRegistryData[] installedApps;

            foreach (KeyValuePair<string, string> pair in allInstallers)
            {
                ApplicationRegistryData data = e.GetInstallInfo(pair.Value);

                Console.WriteLine("reg.appName: {0}", data.appName);
                Console.WriteLine("reg.installLocation: {0}", data.installLocation);
                Console.WriteLine("reg.displayVersion: {0}", data.displayVersion);

                // hopefully we've now got a bunch of ApplicationRegistryData
                // objects with names, install locations and versions
                //if (target != null)
                //{
                //    //Console.WriteLine("target: {0}", target);
                //    try
                //    {
                //        string srcDirRoot = Path.Combine(e.ExtractDir, e.SourceDir);
                //        string origin = Path.Combine(srcDirRoot, pair.Key);
                //        e.run(origin, target);
                //    }
                //    catch (UnauthorizedAccessException)
                //    {
                //        MessageBox.Show("Clyde must be run as Administrator on this system", "sorry");
                //        throw;
                //    }
                //}
            }
            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
