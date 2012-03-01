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
            ETApplication avPlayer = new ETApplication("AVPlayer", "Envision Web Apps", true);
            ETApplication recordingDownloadTool = new ETApplication("RecordingDownloadTool", "Envision Web Apps", true);
            ETApplication wmWrapperService = new ETApplication("WMWrapperService", "Envision Windows Media Wrapper Service");
            ETApplication dbMigration = new ETApplication("DBMigration", "Envision Database Migration");


            // 9.10 and 10.0 installers
            Installer serverInstaller = new Installer("Server", "Envision Server");
            serverInstaller.applications.Add(server);
            serverInstaller.applications.Add(channelManager);

            Installer centricityInstaller = new Installer("Centricity", "Envision Centricity");
            centricityInstaller.applications.Add(centricity);

            Installer webAppsInstaller = new Installer("WebApps", "Envision Web Apps");
            webAppsInstaller.applications.Add(avPlayer);
            webAppsInstaller.applications.Add(recordingDownloadTool);

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

                if (i.isInstalled)
                {
                    // debug
                    logger.Info("i.abbr: {0}", i.abbr);
                    logger.Info("i.displayName: {0}", i.displayName);
                    logger.Info("i.installLocation: {0}", i.installLocation);
                    logger.Info("i.displayVersion: {0}", i.displayVersion);

                    // patch each application in the given installation separately
                    for (int j = 0; j < i.applications.Count; j++)
                    {
                        // origin is    e.ExtractDir                + e.SourceDir   + toPatch.abbr
                        // e.g.         C:\patch_staging\10.1.14.9\ + patchFiles    + Server
                        string tmp = Path.Combine(e.ExtractDir, e.SourceDir);
                        string origin = Path.Combine(tmp, i.applications[j].abbr);
                        string target = i.installLocation;
                        bool replaceAll = i.applications[j].replaceAll;
                        logger.Info("[debug] e.run(origin, target, replaceAll)\n\torigin={0}\n\ttarget={1}\n\treplaceAll={2}", origin, target, replaceAll);

                        if (Directory.Exists(origin))
                        {
                            e.run(origin, target, replaceAll);
                        }
                        else
                        {
                            logger.Info("Directory {0} not found.  Skipping", origin);
                        }
                    }
                    Console.WriteLine();
                }
            }

            // TC: for testing
            Console.Write("Press ENTER to continue");
            Console.ReadLine();
        }
    }
}
