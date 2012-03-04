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
            Console.Write("(attach to Clyde.exe then) press ENTER to continue: ");
            Console.ReadLine();

            // Clyde's prime directive is to patch the ET applications on this
            // host.  The recipe:
            //
            // == Get everything ready ==
            // 1: [done] Create ETApplication and Installer objects for all applications and installers
            // 2: Create Patch objects from the contents of the patch
            // 3: Create a Dictionary of Patches and their corresponding ETApplications
            //    (Unused ETApplications are not included.)
            //
            // == The real work begins ==
            // 1: Inspect each (key, value) pair in the patch Dictionary
            // 2: Create backup folders; copy the new and original files
            // 3: Copy the new files to the original location

            // applications
            ETApplication server = new ETApplication("Server", "Envision Server");
            ETApplication channelManager = new ETApplication("ChannelManager", "Envision Channel Manager");
            ETApplication centricity = new ETApplication("Centricity", "Envision Centricity");
            ETApplication avPlayer = new ETApplication("AVPlayer", "Envision Web Apps", "AVPlayer");
            ETApplication recordingDownloadTool = new ETApplication("RecordingDownloadTool", "Envision Web Apps", "RecordingDownloadTool");
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


            Extractor e = new Extractor();

            // get the shallow list of directories under patch_staging\<version>\patchFiles
            string patchBasePath = Path.Combine(e.ExtractDir, e.SourceDir);
            string[] cache = Directory.GetDirectories(patchBasePath, "*", SearchOption.TopDirectoryOnly);

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
                    logger.Info("i.name: {0}", i.name);
                    logger.Info("i.displayName: {0}", i.displayName);
                    logger.Info("i.installLocation: {0}", i.installLocation);
                    logger.Info("i.displayVersion: {0}", i.displayVersion);

                    // 1: for each installed app ...
                    foreach (ETApplication installedApp in i.applications)
                    {
                        // 2: if we're patching this app ...
                        for (int j = 0; j < cache.Length; j++)
                        {
                            // hotness
                            string cacheBaseDir = new DirectoryInfo(cache[j]).Name;

                            if (cacheBaseDir /* name */ == installedApp.name)
                            {
                                // 3: add the application's cacheLocation
                                installedApp.cacheLocation = cache[j];
                                logger.Info("app: " + installedApp.name + " cache: " + installedApp.cacheLocation);
                            }
                            else
                            {
                                logger.Info("rejected " + cache[j]);
                            }
                        }
                    }

                    string origin = Path.Combine(e.ExtractDir, e.SourceDir);
                    e.run(origin, i);
                }
            }

            // TC: for testing
            Console.Write("Press ENTER to continue");
            Console.ReadLine();
        }
    }
}
