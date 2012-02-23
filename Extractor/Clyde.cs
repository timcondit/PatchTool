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

            // applications
            ETApplication server = new ETApplication("Server", "Envision Server");
            ETApplication channelManager = new ETApplication("ChannelManager", "Envision Channel Manager");
            ETApplication centricity = new ETApplication("Centricity", "Envision Centricity");
            ETApplication webApps = new ETApplication("WebApps", "Envision Web Apps");
            ETApplication wmWrapperService = new ETApplication("WMWrapperService", "Envision Windows Media Wrapper Service");
            ETApplication dbMigration = new ETApplication("DBMigration", "Envision Database Migration");

            // 9.10 and 10.0 installers
            Installer serverInstaller = new Installer("Server", "Envision Server");
            serverInstaller.applications.Add(server);
            serverInstaller.applications.Add(channelManager);

            Installer centricityInstaller = new Installer("Centricity", "Envision Centricity");
            centricityInstaller.applications.Add(centricity);

            Installer webAppsInstaller = new Installer("WebApps", "Envision Web Apps");
            webAppsInstaller.applications.Add(webApps);

            Installer wmWrapperServiceInstaller = new Installer("WMWrapperService", "Envision Windows Media Wrapper Service");
            wmWrapperServiceInstaller.applications.Add(wmWrapperService);

            Installer dbMigrationInstaller = new Installer("DBMigration", "Envision Database Migration");
            dbMigrationInstaller.applications.Add(dbMigration);

            // 10.1 installers
            Installer serverSuiteInstaller = new Installer("ServerSuite", "Envision Server Suite");
            serverSuiteInstaller.applications.Add(server);
            serverSuiteInstaller.applications.Add(centricity);

            Installer channelManagerInstaller = new Installer("ChannelManager", "Envision Channel Manager");
            channelManagerInstaller.applications.Add(channelManager);

            Installer toolsInstaller = new Installer("Tools", "Envision Tools Suite");
            toolsInstaller.applications.Add(dbMigration);

            // Installer suites by major.minor version seems like the wrong
            // way to go.  We need to update mixed product versions, and that
            // may be too constraining.
            InstallerSuite all = new InstallerSuite();
            all.installers.Add(serverInstaller);
            all.installers.Add(centricityInstaller);
            all.installers.Add(webAppsInstaller);
            all.installers.Add(wmWrapperServiceInstaller);
            all.installers.Add(dbMigrationInstaller);
            all.installers.Add(serverSuiteInstaller);
            all.installers.Add(channelManagerInstaller);
            all.installers.Add(toolsInstaller);

            // get details about installed applications
            foreach (Installer i in all.installers)
            {
                e.GetInstallInfo(i);
                if ((i.displayVersion != null) && (i.installLocation != null))
                {
                    i.isInstalled = true;
                }

                // debug
                if (i.isInstalled)
                {
                    Console.WriteLine("installers.abbr: {0}", i.abbr);
                    Console.WriteLine("installers.displayName: {0}", i.displayName);
                    Console.WriteLine("installers.installLocation: {0}", i.installLocation);
                    Console.WriteLine("installers.displayVersion: {0}", i.displayVersion);
                    Console.WriteLine();
                }
            }

            // debug
            Console.WriteLine("number of installed applications: {0}", all.count);


            // patch installed applications
            //foreach (Installer data in installedAppsInfo)
            //{

            //}

                // hopefully we've now got a bunch of Installer
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

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
