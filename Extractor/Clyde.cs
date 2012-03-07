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
            // TC: for testing
            //Console.Write("(attach to Clyde.exe then) press ENTER to continue: ");
            //Console.ReadLine();

            // applications
            ETApplication server = new ETApplication("Server", "Envision Server");
            ETApplication channelManager = new ETApplication("ChannelManager", "Envision Channel Manager");
            ETApplication radControls = new ETApplication("RadControls", "Envision Centricity");
            radControls.replaceAll = true;
            ETApplication centricity = new ETApplication("Centricity", "Envision Centricity");
            ETApplication avPlayer = new ETApplication("AVPlayer", "Envision Web Apps", "AVPlayer");
            avPlayer.replaceAll = true;
            ETApplication recordingDownloadTool = new ETApplication("RecordingDownloadTool", "Envision Web Apps", "RecordingDownloadTool");
            recordingDownloadTool.replaceAll = true;
            ETApplication wmWrapperService = new ETApplication("WMWrapperService", "Envision Windows Media Wrapper Service");


            // 9.10 and 10.0 installers
            Installer serverInstaller = new Installer("Server", "Envision Server");
            serverInstaller.applications.Add(server);
            serverInstaller.applications.Add(channelManager);

            Installer centricityInstaller = new Installer("Centricity", "Envision Centricity");
            centricityInstaller.applications.Add(centricity);
            centricityInstaller.applications.Add(radControls);

            Installer webAppsInstaller = new Installer("WebApps", "Envision Web Apps");
            webAppsInstaller.applications.Add(avPlayer);
            webAppsInstaller.applications.Add(recordingDownloadTool);

            Installer wmWrapperServiceInstaller = new Installer("WMWrapperService", "Envision Windows Media Wrapper Service");
            wmWrapperServiceInstaller.applications.Add(wmWrapperService);

            Installer dbMigrationInstaller = new Installer("DBMigration", "Envision Database Migration");
            dbMigrationInstaller.applications.Add(new ETApplication("DBMigration", "Envision Database Migration"));

            // 10.1 installers
            Installer serverSuiteInstaller = new Installer("ServerSuite", "Envision Server Suite");
            serverSuiteInstaller.applications.Add(server);
            serverSuiteInstaller.applications.Add(centricity);
            serverSuiteInstaller.applications.Add(radControls);

            Installer channelManagerInstaller = new Installer("ChannelManager", "Envision Channel Manager");
            channelManagerInstaller.applications.Add(channelManager);

            Installer toolsInstaller = new Installer("Tools", "Envision Tools Suite");
            toolsInstaller.applications.Add(new ETApplication("DBMigration", "Envision Database Migration", "DBMigration"));


            InstallerSuite all = new InstallerSuite();
            all.installers.Add(serverInstaller);
            all.installers.Add(centricityInstaller);
            all.installers.Add(webAppsInstaller);
            all.installers.Add(wmWrapperServiceInstaller);
            all.installers.Add(dbMigrationInstaller);
            all.installers.Add(serverSuiteInstaller);
            all.installers.Add(channelManagerInstaller);
            all.installers.Add(toolsInstaller);

            Extractor e = new Extractor();
            List<ETApplication> appsToPatch = new List<ETApplication>();

            // get the shallow list of applications to patch from patch_staging\<version>\patchFiles
            string[] cache = Directory.GetDirectories(e.PatchDir, "*", SearchOption.TopDirectoryOnly);

            foreach (Installer i in all.installers)
            {
                e.GetInstallInfo(i);
                if ((i.displayVersion != null) && (i.installLocation != null))
                {
                    i.isInstalled = true;
                }

                if (i.isInstalled)
                {
                    // 1: for each installed app ...
                    foreach (ETApplication installedApp in i.applications)
                    {
                        // 2: if we're patching this app ...
                        for (int j = 0; j < cache.Length; j++)
                        {
                            if (new DirectoryInfo(cache[j]).Name == installedApp.name)
                            {
                                // 3: add the application's cacheLocation
                                // this seems redundant, or at least not very useful
                                installedApp.installLocation = i.installLocation;
                                installedApp.patchFrom = cache[j];

                                if (installedApp.patchTo != null)
                                {
                                    //logger.Info("(before) installedApp.patchTo: " + installedApp.patchTo);
                                    installedApp.patchTo = Path.Combine(installedApp.installLocation, installedApp.name);
                                    //logger.Info("(after)  installedApp.patchTo: " + installedApp.patchTo);
                                }
                                else
                                {
                                    installedApp.patchTo = installedApp.installLocation;
                                    //logger.Info("installedApp.patchTo: " + installedApp.patchTo);
                                }
                                //installedApp.patchTo = installedApp.installLocation;
                                logger.Info("i.name: {0}", i.name);
                                logger.Info("i.displayName: {0}", i.displayName);
                                logger.Info("i.installLocation: {0}", i.installLocation);
                                logger.Info("i.displayVersion: {0}", i.displayVersion);
                                appsToPatch.Add(installedApp);
                            }
                        }
                    }
                }
            }

            foreach (ETApplication atp in appsToPatch)
            {
                logger.Info("patching: " + atp.displayName);
                e.run(atp);
            }

            // TC: for testing
            Console.Write("Press ENTER to continue");
            Console.ReadLine();
        }
    }
}
