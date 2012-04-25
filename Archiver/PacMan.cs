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

            // This is where we specify which files go into the patch for each application. It will be manually
            // updated for now.
            IEnumerable<string> serverKeys = new List<string>
            {
                "Administrator.exe", "AgentAutomation.dll",
                "AlvasAudio.bat", "AlvasAudio.dll", "AlvasAudio.pdb", "AlvasAudio.tlb",
                "ChannelBrokerService.xml", "CiscoICM.dll",
                "client.properties", "CommonUpdates.xml",
                "ContactSourceRunner.bat", "ContactSources.properties",
                "cstaLoader.dll", "cstaLoader.pdb",
                "cstaLoader_1_2.dll", "cstaLoader_1_3_3.dll",
                "cstaLoader_3_33.dll", "cstaLoader_6_4_3.dll",
                "cstaLoader_9_1.dll", "cstaLoader_9_5.dll",
                "ctcLoader_6.0.dll", "ctcLoader_6_0.pdb",
                "ctcLoader_7.0.dll", "ctcLoader_7_0.pdb",
                "DBServiceCentricityWfm.xml",
                "Envision.jar",
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
                "server.dll", "server.pdb",
                "SIP_events.properties",
                "SourceRunnerService.exe", "SourceRunnerService.pdb",
                "TeliaCallGuide.dll", "TeliaCallGuide.pdb",
                "TokenService.xml",
                "Tsapi.dll", "Tsapi.pdb", "updater.jar",
                "WMWrapperService.xml",

                // DatabaseUpdates
                "C2CUpdates.xml", "CommonUpdates.xml", "EWFMUpdates.xml",
                "manifest.xml_6", "SpeechUpdates.xml",
                "MSSQLUpdate_build_9.12.0.28.xml",
                "MSSQLUpdate_build_9.12.0.37.xml",
                "MSSQLUpdate_build_9.12.0.38.xml",
                "MSSQLUpdate_build_10.0.0.22.xml",
                "MSSQLUpdate_build_10.0.0.31.xml",
                "MSSQLUpdate_build_10.0.0303.1.xml",
                "MSSQLUpdate_build_10.0.1.1.xml", "MSSQLUpdate_build_10.0.1.1.xml_1",
                "MSSQLUpdate_build_10.1.0.47.xml",
                "MSSQLUpdate_build_10.1.0.61.xml",
                "MSSQLUpdate_build_10.1.0.62.xml",
                "MSSQLUpdate_build_10.1.0.65.xml", "MSSQLUpdate_build_10.1.0.65.xml_1",
                "MSSQLUpdate_build_10.1.0.99.xml",
                "MSSQLUpdate_build_10.1.0.140.xml",
                "MSSQLUpdate_build_10.1.0.151.xml",
                "MSSQLUpdate_build_10.1.0.172.xml",
                "MSSQLUpdate_build_10.1.0.201.xml",
                "MSSQLUpdate_build_10.1.0.236a.xml", "MSSQLUpdate_build_10.1.0.236b.xml",
                "MSSQLUpdate_build_10.1.0.242.xml",
                "MSSQLUpdate_build_10.1.0.333.xml",
                "MSSQLUpdate_build_10.1.2.0.xml",

                // LAA-BIN
                "dumpbin.exe", "EnvisionServer.exe", "java.exe", "javaw.exe",
                
                // for registering AlvasAudio.dll
                "gacutil.exe", "regasm.exe",

                // documentation
                "Centricity_Webhelp_DE", "Centricity_Webhelp_EN", "Centricity_Webhelp_ES",
            };

            IEnumerable<string> cmKeys = new List<string>
            {
                "AlvasAudio.bat", "AlvasAudio.dll", "AlvasAudio.pdb", "AlvasAudio.tlb",
                "audiocodesChannel.dll", "audiocodesChannel.pdb",
                "AudioReader.dll", "AudioReader.pdb",
                "AvayaVoipChannel.dll", "AvayaVoipChannel.pdb",
                "Chanmgr_common.xsd", "ChanMgrSvc.exe", "ChanMgrSvc.pdb",
                "ChanMgrSvc.SIP.config",
                "ChannelManager.ICM.xml", "ChannelManager.SIP.xml",
                "cleanup-SIPGateway-dir.bat",
                "DemoModeChannel.dll", "DemoModeChannel.pdb",
                "DialogicChannel.dll", "DialogicChannel.pdb",
                "DialogicChannel60.dll", "DialogicChannel60.pdb",
                "DMCCConfigLib.dll", "DMCCConfigLib.pdb",
                "dmcc_devices.bat",
                "DMCCWrapperLib.dll", "DMCCWrapperLib.pdb", "DMCCWrapperLib.tlb", 
                "IPXChannel.dll", "IPXChannel.pdb",
                "LumiSoft.Net.dll", "LumiSoft.Net.pdb", "LumiSoft.Net.xml",
                "RtpTransmitter.dll", "RtpTransmitter.pdb",
                "server.dll", "server.pdb",
                "Set_ChannelDeviceIds.sql",
                "SIPChannel.dll", "SIPChannel.pdb",
                "SIPConfigLib.dll", "SIPConfigLib.pdb",
                "SIP_events.properties",
                "SIPPhone.dll", "SIPPhone.pdb",
                "SIPWrapperLib.dll", "SIPWrapperLib.pdb",
                "states.BIB.xml",
                
                // EnvisionSR
                "EnvisionSR.bat", "EnvisionSR.reg", "instsrv.exe",
                "sleep.exe", "srvany.exe", "svcmgr.exe",
                
                // SIP Gateway
                "SIPWrapperLib.tlb", "SIPWrapperLogging.xml",

                // for registering AlvasAudio.dll
                "gacutil.exe", "regasm.exe",
            };

            IEnumerable<string> ctKeys = new List<string>
            {
                "AgentInboxGrid.ascx",
                "Agents.aspx", "Agents.aspx.resx", "Agents.aspx.de.resx", "Agents.aspx.es.resx",
                "App_Code.compiled", "App_global.asax.compiled", "App_GlobalResources.compiled",
                "AttachedTrainingClipGrid.ascx",
                "centricity.dll", "centricity.pdb",
                "Centricity_BLL.dll", "Centricity_BLL.pdb", "Centricity_BLL.XmlSerializers.dll",
                "Centricity_DAL.dll", "Centricity_DAL.pdb",
                "Centricity_deploy.resources.dll_DE", "Centricity_deploy.resources.dll_DE_1",
                "Centricity_deploy.resources.dll_ES",
                "Centricity_deploy.dll", "Centricity_SCA.dll",
                "CentricityMaster.master.resx", "CentricityMaster.master.de.resx", "CentricityMaster.master.es.resx",
                "Centricity_Shared.dll", "Centricity_Shared.pdb",
                "Create_Centricity_WFM_SPROCS.sql",
                "Default.aspx",
                "EditEvaluation.aspx", "EnvisionTheme.css",
                "EvaluationGrid.ascx",
                "NewEvaluation.aspx", "RadEditor.skin",
                "RAL.dll", "RAL.pdb",
                "RecognitionDashboardItem.ascx", "Recognitions.aspx",
                "RecordingGridToolbar.ascx", "RecordingGridToolbar.ascx.resx", "RecordingGridToolbar.ascx.de.resx", "RecordingGridToolbar.ascx.es.resx",
                "SiteToGroupAgentMover.ascx", "SiteToGroupAgentMover.ascx.resx", "SiteToGroupAgentMover.ascx.de.resx", "SiteToGroupAgentMover.ascx.es.resx",
                "Telerik.Web.Design.dll", "Telerik.Web.UI.dll", "Telerik.Web.UI.xml",
                "TrainingClipGrid.ascx",
            };

            IEnumerable<string> wmwsKeys = new List<string>
            {
                "DefaultEnvisionProfile.prx", "server.dll", "WMWrapperService.exe",
            };

            IEnumerable<string> avplayerKeys = new List<string>
            {
                "AVPlayer.application", "AgentSupport.exe.deploy",
                "AVPlayer.exe.config.deploy", "AVPlayer.exe.deploy", "AVPlayer.exe.manifest",
                "CentricityApp.dll.deploy", "hasp_windows.dll.deploy", "Interop.WMPLib.dll.deploy",
                "log4net.dll.deploy", "nativeServiceWin32.dll.deploy",
                "server.dll.deploy", "SharedResources.dll.deploy", "ISource.dll.deploy",
                "AVPlayer.resources.dll.deploy", "AVPlayer.resources.dll.deploy_1",
                "CentricityApp.resources.dll.deploy", "CentricityApp.resources.dll.deploy_1",
                "AVPlayerIcon.ico.deploy",
            };

            IEnumerable<string> rdtoolKeys = new List<string>
            {
                "RecordingDownloadTool.application", "CentricityApp.dll.deploy_1", "log4net.dll.deploy_1",
                "RecordingDownloadTool.exe.config.deploy", "RecordingDownloadTool.exe.deploy",
                "RecordingDownloadTool.exe.manifest", "server.dll.deploy_1", "sox.exe.deploy",
                "CentricityApp.resources.dll.deploy_2", "CentricityApp.resources.dll.deploy_3",
                "RecordingDownloadTool.resources.dll.deploy", "RecordingDownloadTool.resources.dll.deploy_1",
            };

            IEnumerable<string> dbmigrationKeys = new List<string>
            {
                "DBMigration_84SP9_To_10.sql",
            };

            // NB: the app names (WebApps, Server, ...) must match the names of the IConfigs in PatchLib
            logger.Info("Copying Envision Server files");
            a.makePortablePatch("Server", serverKeys);

            logger.Info("Copying Channel Manager files");
            a.makePortablePatch("ChannelManager", cmKeys);

            logger.Info("Copying RadControls files");
            a.makePortablePatch("RadControls", @"..\..\..\..\..\..\workdir\centricity\ET\RadControls\2011_Q1");

            logger.Info("Copying Centricity files");
            a.makePortablePatch("Centricity", ctKeys);

            logger.Info("Copying AV Player files");
            a.makePortablePatch("AVPlayer", avplayerKeys);

            logger.Info("Copying Recording Download Tool files");
            a.makePortablePatch("RecordingDownloadTool", rdtoolKeys);

            logger.Info("Copying Windows Media Wrapper Service files");
            a.makePortablePatch("WMWrapperService", wmwsKeys);

            logger.Info("Copying Database Migration files");
            a.makePortablePatch("DBMigration", dbmigrationKeys);

            // The extract dir is set before the archive is created.  As far as I know, there is nothing to be done at
            // extraction time to change that.  Bottom line is, the extractDir cannot be APPDIR.
            a.ExtractDir = Path.Combine(@"C:\patch_staging", a.PatchVersion);
            a.run();
        }
    }
}
