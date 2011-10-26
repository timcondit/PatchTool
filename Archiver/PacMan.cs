using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NLog;

namespace PatchTool
{
    public class PacMan
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Archiver a = new Archiver();
            a.SourceDir = "patchFiles";

            string webapps_version = Archiver.formatVersionString(a.PatchVersion);
            a.makeSourceConfig(webapps_version);
            a.makeTargetConfig(webapps_version);

            // Should have the patchVersion before calling a.makePortablePatch().  Actually, should really break the
            // configuration out into a separate utility.

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            IEnumerable<string> serverKeys = new List<string> {
                "Administrator.exe", "AgentAutomation.dll",
                "AlvasAudio.bat", "AlvasAudio.dll", "AlvasAudio.pdb", "AlvasAudio.tlb",
                "ChannelBrokerService.xml", "CiscoICM.dll",
                "client.properties",
                "CommonUpdates.xml", "ContactSources.properties",
                "cstaLoader.dll", "cstaLoader.pdb",
                "cstaLoader_1_2.dll", "cstaLoader_1_3_3.dll",
                "cstaLoader_3_33.dll", "cstaLoader_6_4_3.dll",
                "cstaLoader_9_1.dll", "cstaLoader_9_5.dll",
                "ctcLoader_6.0.dll", "ctcLoader_6_0.pdb",
                "ctcLoader_7.0.dll", "ctcLoader_7_0.pdb",
                "Default.aspx", "Envision.jar",
                "EditEvaluation.aspx", "EnvisionTheme.css",
                "envision_schema.xml", "envision_schema_central.xml",
                "EnvisionControls.cab",
                "EnvisionServer.bat", "EnvisionServer.exe_1",
                "ETContactSource.exe", "ETContactSource.pdb",
                "ETScheduleService.xml", "ETService.exe",
                "jtapi.jar", "JtapiItemService.xml", "jtracing.jar",
                "log4j.properties.template",
                "manifest.xml", "manifest.xml_2",
                "MSSQLUpdate_build_10.0.0303.1.xml", "MSSQLUpdate_build_10.1.2.0.xml",
                "nativeServiceWin32.dll",
                "NetMerge.dll", "NetMerge.pdb",
                "NewEvaluation.aspx", "RadEditor.skin",
                "server.dll", "server.pdb",
                "SIP_events.properties",
                "SiteToGroupAgentMover.ascx", "SiteToGroupAgentMover.ascx.resx",
                "SiteToGroupAgentMover.ascx.de.resx", "SiteToGroupAgentMover.ascx.es.resx",
                "SourceRunnerService.exe", "SourceRunnerService.pdb",
                "TeliaCallGuide.dll", "TeliaCallGuide.pdb",
                "TokenService.xml",
                "Tsapi.dll", "Tsapi.pdb", "updater.jar",

                // Centricity
                "App_Code.compiled", "App_global.asax.compiled", "App_GlobalResources.compiled",
                "centricity.dll", "centricity.pdb",
                "Centricity_BLL.dll", "Centricity_BLL.pdb",
                "Centricity_BLL.XmlSerializers.dll",
                "Centricity_DAL.dll", "Centricity_DAL.pdb",
                "Centricity_deploy.resources.dll_DE", "Centricity_deploy.resources.dll_DE_1",
                "Centricity_deploy.resources.dll_ES",
                "Centricity_deploy.dll", "Centricity_SCA.dll",
                "Centricity_Shared.dll", "Centricity_Shared.pdb",
                "RAL.dll", "RAL.pdb",

                // LAA-BIN
                "dumpbin.exe", "EnvisionServer.exe", "java.exe", "javaw.exe",
                
                // for registering AlvasAudio.dll
                "gacutil.exe", "regasm.exe",

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
                "SIP_events.properties",
                "SIPPhone.dll", "SIPPhone.pdb",
                "SIPWrapperLib.dll", "SIPWrapperLib.pdb", "SIPWrapperLib.tlb",
                
                // EnvisionSR
                "EnvisionSR.bat", "EnvisionSR.reg", "instsrv.exe",
                "sleep.exe", "srvany.exe", "svcmgr.exe",
                
                // SIP Gateway
                "GatewayLib.dll", "GatewayLib.pdb", "GatewayLogging.xml",
                "InstallUtil.exe", "InstallSIPGateway.bat",
                "log4net.dll",
                "LumiSoft.Net.dll", "LumiSoft.Net.xml", "LumiSoft.Net.pdb",
                "sc.exe", "server.dll", "server.pdb",
                "SIPGateway.exe", "SIPGateway.exe.config", "SIPGateway.pdb",
                "UninstallSIPGateway.bat",

                // for registering AlvasAudio.dll
                "gacutil.exe", "regasm.exe",
            };

            // This is where we specify which files go into the patch (WMWrapperService). It will be manually updated
            // for now.
            IEnumerable<string> wmwsKeys = new List<string> {
                "DefaultEnvisionProfile.prx", "server.dll", "WMWrapperService.exe",
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

            // The extract dir is set before the archive is created.  As far as I know, there is nothing to be done at
            // extraction time to change that.  Bottom line is, the extractDir cannot be APPDIR.
            a.ExtractDir = Path.Combine(@"C:\patch_staging", a.PatchVersion);
            a.run();
        }
    }
}
