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

            foreach (KeyValuePair<string, string> pair in allInstallers)
            {
                string target = e.GetInstallLocation(pair.Value);
                if (target != null)
                {
                    try
                    {
                        string srcDirRoot = Path.Combine(e.ExtractDir, e.SourceDir);
                        string origin = Path.Combine(srcDirRoot, pair.Key);
                        e.run(origin, target);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Clyde must be run as Administrator on this system", "sorry");
                        throw;
                    }
                }
            }
            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
