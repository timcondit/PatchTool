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

            // Get the intersection of those applications which are patched with those which are installed.  For
            // example, if Server, ChannelManager and Tools are patched, but only Server and ChannelManager are
            // installed, then we don't patch Tools.  But it may be staged if it's easier to do it than not.

            IDictionary<string, string> allInstallers = new Dictionary<string, string>();
            allInstallers.Add("Server", "Envision Server");
            allInstallers.Add("ServerSuite", "Envision Server Suite");
            allInstallers.Add("ChannelManager", "Envision Channel Manager");
            allInstallers.Add("Centricity", "Envision Centricity");
            allInstallers.Add("WebApps", "Envision Web Apps");
            allInstallers.Add("WMWrapperService", "Envision Windows Media Wrapper Service");
            allInstallers.Add("DBMigration", "Envision Database Migration");
            allInstallers.Add("Tools", "Envision Tools Suite");

            // first check
            IDictionary<string, string> installedApps = e.getInstalledApps(allInstallers.Keys);

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
