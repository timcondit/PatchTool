using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using NLog;

namespace PatchTool
{
    public class PacMan
    {
        private sealed class Options
        {
            #region Standard Option Attribute
            [Option("r", "patchVersion",
                    Required = true,
                    HelpText = "The version number for this patch.")]
            public string patchVersion = "0.0.0.0";

            [HelpOption(
                    HelpText = "Display this help screen.")]

            public string GetUsage()
            {
                var help = new HelpText("Envision Package Manager");
                help.AdditionalNewLineAfterOption = true;
                help.Copyright = new CopyrightInfo("Envision Telephony, Inc.", 2011);
                help.AddPreOptionsLine("Usage: PacMan -r<patchVersion>");
                help.AddPreOptionsLine("       PacMan -?");
                help.AddOptions(this);

                return help;
            }
            #endregion
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // TODO use patchVersion as the sourceDir; get rid of sourceDir
            if (args.Length < 1)
            {
                string usage = "PacMan.exe\n\n";
                usage += "Required:\n";
                usage += "\t--patchVersion\tthe version number for this patch\n";
                MessageBox.Show(usage, "PacMan needs more info");

                logger.Info("Not enough arguments provided.  Show usage and exit.");
            }

            Archiver a = new Archiver();

            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            a.SourceDir = "patchFiles";

            if (options.patchVersion == String.Empty)
            {
                // "pretty it up" and exit
                throw new ArgumentException("something's broken! (options.patchVersion)");
            }
            else
            {
                a.PatchVersion = options.patchVersion;
            }

            string webapps_version = Archiver.formatVersionString(a.PatchVersion);

            a.makeSourceConfig(webapps_version);
            a.makeTargetConfig(webapps_version);

            // Should have the patchVersion before calling a.makePortablePatch().  Actually, should really break the
            // configuration out into a separate utility.

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            IEnumerable<string> serverKeys = new List<string> {
                "Envision.jar", "envision_schema.xml", "envision_schema_central.xml",
                "ETScheduleService.xml", "ChannelBrokerService.xml", "CiscoICM.dll",
                "cstaLoader.dll", "cstaLoader_1_2.dll", "cstaLoader_1_3_3.dll", "cstaLoader_3_33.dll",
                "cstaLoader_9_1.dll", "cstaLoader_9_5.dll", "ctcLoader_6.0.dll", "ctcLoader_7.0.dll",
                "EditEvaluation.aspx", "EnvisionTheme.css", "NetMerge.dll", "NewEvaluation.aspx",
                "RadEditor.skin", "SourceRunnerService.exe", "TeliaCallGuide.dll", "Tsapi.dll",
                "CommonUpdates.xml", "MSSQLUpdate_build_10.0.0303.1.xml",

                // Centricity
                "centricity.dll", "Centricity_BLL.dll", "Centricity_DAL.dll", "RAL.dll",
            };

            // This is where we specify which files go into the patch (ChannelManager). It will be manually updated
            // for now.
            IEnumerable<string> cmKeys = new List<string> {
                "AlvasAudio.bat", "AlvasAudio.dll", "AlvasAudio.pdb", "AlvasAudio.tlb",
                "audiocodesChannel.dll", "audiocodesChannel.pdb",
                "AudioReader.dll", "AudioReader.pdb",
                "AvayaVoipChannel.dll", "AvayaVoipChannel.pdb",
                "ChanMgrSvc.exe", "ChanMgrSvc.pdb",
                "DemoModeChannel.dll", "DemoModeChannel.pdb",
                "DialogicChannel.dll", "DialogicChannel.pdb",
                "DialogicChannel60.dll", "DialogicChannel60.pdb",
                "DMCCConfigLib.dll", "DMCCConfigLib.pdb",
                "DMCCWrapperLib.dll", "DMCCWrapperLib.pdb", "DMCCWrapperLib.tlb", 
                "IPXChannel.dll", "IPXChannel.pdb",
                "LumiSoft.Net.dll", "LumiSoft.Net.pdb",
                "RtpTransmitter.dll", "RtpTransmitter.pdb",
                "server.dll", "server.pdb",
                "SIPChannel.dll", "SIPChannel.pdb",
                "SIPChannelHpxMedia.dll", "SIPChannelHpxMedia.pdb",
                "SIPConfigLib.dll", "SIPConfigLib.pdb",
                "SIPPhone.dll", "SIPPhone.pdb",
                "SIPWrapperLib.dll", "SIPWrapperLib.pdb", "SIPWrapperLib.tlb",
                
                // EnvisionSR
                "EnvisionSR.bat", "EnvisionSR.reg", "instsrv.exe",
                "sleep.exe", "srvany.exe", "svcmgr.exe",
                
                // SIP Gateway
                "GatewayLib.dll", "GatewayLib.pdb", "GatewayLogging.xml",
                "log4net.dll",
                "LumiSoft.Net.dll", "LumiSoft.Net.xml", "LumiSoft.Net.pdb",
                "server.dll", "server.pdb",
                "SIPGateway.exe", "SIPGateway.exe.config", "SIPGateway.pdb",

                // for registering AlvasAudio.dll
                "gacutil.exe", "regasm.exe",
            };

            // This is where we specify which files go into the patch (WMWrapperService). It will be manually updated
            // for now.
            IEnumerable<string> wmwsKeys = new List<string> {
                "DefaultEnvisionProfile.prx",
            };

            // This is where we specify which files go into the patch (CentricityWebApplications). It will be manually
            // updated for now.
            IEnumerable<string> webappsKeys = new List<string> {
                // AVPlayer
                "AVPlayer.application", "AgentSupport.exe.deploy",
                "AVPlayer.exe.config.deploy", "AVPlayer.exe.deploy", "AVPlayer.exe.manifest",
                "CentricityApp.dll.deploy", "hasp_windows.dll.deploy", "Interop.WMPLib.dll.deploy",
                "log4net.dll.deploy", "nativeServiceWin32.dll.deploy",
                "server.dll.deploy", "SharedResources.dll.deploy", "ISource.dll.deploy",
                "AVPlayer.resources.dll.deploy", "AVPlayer.resources.dll.deploy_1",
                "CentricityApp.resources.dll.deploy", "CentricityApp.resources.dll.deploy_1",
                "AVPlayerIcon.ico.deploy",

                // RecordingDownloadTool
                "RecordingDownloadTool.application", "CentricityApp.dll.deploy_1", "log4net.dll.deploy_1",
                "RecordingDownloadTool.exe.config.deploy", "RecordingDownloadTool.exe.deploy",
                "RecordingDownloadTool.exe.manifest", "server.dll.deploy_1", "sox.exe.deploy",
                "CentricityApp.resources.dll.deploy_2", "CentricityApp.resources.dll.deploy_3",
                "RecordingDownloadTool.resources.dll.deploy", "RecordingDownloadTool.resources.dll.deploy_1",
            };

            // This is where we specify which files go into the patch (Tools). It will be manually updated for now.
            IEnumerable<string> toolsKeys = new List<string> {
                "DBMigration_84SP9_To_10.sql",
            };

            // NB: the app names (WebApps, Server, ...) must match the names of the IConfigs in PatchLib
            logger.Info("Copying ServerSuite patch files");
            a.makePortablePatch("Server", serverKeys);

            logger.Info("Copying ChannelManager patch files");
            a.makePortablePatch("ChannelManager", cmKeys);

            logger.Info("Copying CentricityWebApps patch files");
            a.makePortablePatch("WebApps", webappsKeys);

            logger.Info("Copying WMWrapperService patch files");
            a.makePortablePatch("WMWrapperService", wmwsKeys);

            logger.Info("Copying Tools patch files");
            a.makePortablePatch("Tools", toolsKeys);

            // TC: If the files are stored in C:\patch_staging\<APPNAME>\<PATCHVER>, and that location already exists,
            // error and exit.
            //
            // The extract dir is set before the archive is created.  There is NOTHING that can be done (as far as I
            // know) at extraction time to change that.  Bottom line is, the extractDir cannot be APPDIR.
            a.ExtractDir = Path.Combine(@"C:\patch_staging", a.PatchVersion);
            a.run();
        }
    }
}
