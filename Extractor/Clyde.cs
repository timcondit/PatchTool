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
            ETApplication server = new ETApplication();
            ETApplication channelManager = new ETApplication();
            ETApplication centricity = new ETApplication();
            ETApplication webApps = new ETApplication();
            ETApplication wmWrapperService = new ETApplication();
            ETApplication dbMigration = new ETApplication();


            // 9.10 and 10.0 installers
            Installer serverInstaller = new Installer();
            serverInstaller.applications.Add(server);
            serverInstaller.applications.Add(channelManager);

            Installer centricityInstaller = new Installer();
            centricityInstaller.applications.Add(centricity);

            Installer webAppsInstaller = new Installer();
            webAppsInstaller.applications.Add(webApps);

            Installer wmWrapperServiceInstaller = new Installer();
            wmWrapperServiceInstaller.applications.Add(wmWrapperService);

            Installer dbMigrationInstaller = new Installer();
            dbMigrationInstaller.applications.Add(dbMigration);

            // 10.1 installers
            Installer serverSuiteInstaller = new Installer();
            serverSuiteInstaller.applications.Add(server);
            serverSuiteInstaller.applications.Add(centricity);

            Installer channelManagerInstaller = new Installer();
            channelManagerInstaller.applications.Add(channelManager);

            Installer toolsInstaller = new Installer();
            toolsInstaller.applications.Add(dbMigration);


            // installers suites (by major.minor version)
            InstallerSuite nineDotTen = new InstallerSuite();
            nineDotTen.major_minor = "9.10";
            nineDotTen.installers.Add(serverInstaller);
            nineDotTen.installers.Add(centricityInstaller);
            nineDotTen.installers.Add(webAppsInstaller);
            nineDotTen.installers.Add(wmWrapperServiceInstaller);
            nineDotTen.installers.Add(dbMigrationInstaller);

            InstallerSuite tenDotZero = new InstallerSuite();
            tenDotZero.major_minor = "10.0";
            tenDotZero.installers.Add(serverInstaller);
            tenDotZero.installers.Add(centricityInstaller);
            tenDotZero.installers.Add(webAppsInstaller);
            tenDotZero.installers.Add(wmWrapperServiceInstaller);
            tenDotZero.installers.Add(dbMigrationInstaller);

            InstallerSuite tenDotOne = new InstallerSuite();
            tenDotOne.major_minor = "10.1";
            tenDotOne.installers.Add(serverSuiteInstaller);
            tenDotOne.installers.Add(channelManagerInstaller);
            tenDotOne.installers.Add(webAppsInstaller);
            tenDotOne.installers.Add(wmWrapperServiceInstaller);
            tenDotOne.installers.Add(toolsInstaller);


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

            List<Installer> installedAppsInfo = new List<Installer>();

            // get details about installed applications
            foreach (KeyValuePair<string, string> pair in allInstallers)
            {
                Installer data = e.GetInstallInfo(pair.Key, pair.Value);
                if ((data.abbr != null) &&
                    (data.displayName != null) &&
                    (data.displayVersion != null) &&
                    (data.installLocation != null))
                {
                    installedAppsInfo.Add(data);
                }

                // debug
                Console.WriteLine("installer.abbr: {0}", data.abbr);
                Console.WriteLine("installer.displayName: {0}", data.displayName);
                Console.WriteLine("installer.installLocation: {0}", data.installLocation);
                Console.WriteLine("installer.displayVersion: {0}", data.displayVersion);
                Console.WriteLine();
            }

            // patch installed applications
            foreach (Installer data in installedAppsInfo)
            {

            }

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

            // debug
            Console.WriteLine("installedAppsInfo.Count: {0}", installedAppsInfo.Count);

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
