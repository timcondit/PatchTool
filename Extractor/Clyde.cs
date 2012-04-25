using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace PatchTool
{
    class Clyde
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Extractor e = new Extractor();
            e.CheckETServices();

            // TC: for testing
            //Console.Write("(attach to Clyde.exe then) press ENTER to continue: ");
            //Console.ReadLine();

            // applications
            ETApplication server = new ETApplication("Server", "Envision Server");
            ETApplication channelManager = new ETApplication("ChannelManager", "Envision Channel Manager");
            ETApplication radControls = new ETApplication("RadControls", "Envision Centricity", "RadControls");
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


            // get the shallow list of applications to patch from patch_staging\<version>\patchFiles
            string[] cache = Directory.GetDirectories(e.PatchDir, "*", SearchOption.TopDirectoryOnly);

            List<ETApplication> appsToPatch = new List<ETApplication>();
            List<Installer> installsToBackup = new List<Installer>();

            foreach (Installer i in all.installers)
            {
                e.GetInstallInfo(i);
                if ((i.displayVersion != null) && (i.installLocation != null))
                {
                    i.isInstalled = true;
                    installsToBackup.Add(i);
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
                                    installedApp.patchTo = Path.Combine(installedApp.installLocation, installedApp.name);
                                }
                                else
                                {
                                    installedApp.patchTo = installedApp.installLocation;
                                }
                                appsToPatch.Add(installedApp);
                            }
                        }
                    }
                }
            }

            if (installsToBackup.Count > 0)
            {
                List<string> skipList = new List<string>();
                skipList.Add("etDailyLog");
                e.Backup(installsToBackup, skipList);
            }
            if (appsToPatch.Count > 0)
            {
                e.Patch(appsToPatch);
            }

            // TC: for testing
            Console.Write("Press ENTER to continue");
            Console.ReadLine();
        }
    }
}
