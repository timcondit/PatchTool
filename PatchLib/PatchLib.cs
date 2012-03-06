using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Win32;
using Nini.Config;
using NLog;
using com.et.versioninfo;

// using DotNetZip library
// http://dotnetzip.codeplex.com/
// http://cheeso.members.winisp.net/DotNetZipHelp/html/d4648875-d41a-783b-d5f4-638df39ee413.htm
//
// TODO - maybe
// 1: look at ExtractExistingFileAction OverwriteSilently
//  http://cheeso.members.winisp.net/DotNetZipHelp/html/5443c4c0-6f74-9ae1-37fd-9a4ae936832d.htm
// 2: add rollback
// 3: add undo (like rollback only after the patch completes)
// 4: add continue / cancel "breakpoints"
// 5: add "list file contents" to the archives (e.g. APP-VER.exe)
// 6: add logging when creating archives


namespace PatchTool
{
    public class Archiver
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string _sourceDir;
        public string SourceDir
        {
            get { return _sourceDir; }
            set { _sourceDir = value; }
        }

        private string _patchVersion = VersionInfo.PRODUCT_VERSION;
        public string PatchVersion
        {
            get { return _patchVersion; }
        }

        private string _extractDir = ".";
        public string ExtractDir
        {
            get { return _extractDir; }
            set { _extractDir = value; }
        }

        // Source config is a list of where to find files on the Aristotle working copy.  It is independent of the
        // apps to patch.  Every file in makeTargetConfig (identified by key) must be here.
        public void makeSourceConfig(string webapps_version = "0_0_0_0")
        {
            IniConfigSource source = new IniConfigSource();
            IConfig config = source.AddConfig("Sources");

            // from src\tools\PatchTool\Archiver\bin\Release back to the working copy root
            config.Set("srcRoot", @"..\..\..\..\..\..");
            config.Set("webapps_version", webapps_version);

            // If there's a second file with the same name and different path, we'll need to append something (maybe
            // "_1") to the end of the second file of the same name.  But we can't append a mangled file name to the
            // path to identify the file.

            // from the working copy
            config.Set("Administrator.exe", @"${srcRoot}\src\clients\admin\Administrator\Release\Administrator.exe");
            config.Set("AgentAutomation.dll", @"${srcRoot}\src\apis\automationapi\Release\AgentAutomation.dll");
            config.Set("AlvasAudio.dll", @"${srcRoot}\workdir\SharedResources\AlvasAudio.dll");
            config.Set("AlvasAudio.pdb", @"${srcRoot}\workdir\SharedResources\AlvasAudio.pdb");
            config.Set("AlvasAudio.tlb", @"${srcRoot}\workdir\SharedResources\AlvasAudio.tlb");
            config.Set("AlvasAudio.bat", @"${srcRoot}\config\chanmgr\AlvasAudio.bat");
            config.Set("App_Code.compiled", @"${srcRoot}\workdir\centricity\ET\bin\App_Code.compiled");
            config.Set("App_global.asax.compiled", @"${srcRoot}\workdir\centricity\ET\bin\App_global.asax.compiled");
            config.Set("App_GlobalResources.compiled", @"${srcRoot}\workdir\centricity\ET\bin\App_GlobalResources.compiled");
            config.Set("audiocodesChannel.dll", @"${srcRoot}\workdir\ChannelManager\audiocodesChannel.dll");
            config.Set("audiocodesChannel.pdb", @"${srcRoot}\workdir\ChannelManager\audiocodesChannel.pdb");
            config.Set("AudioReader.dll", @"${srcRoot}\workdir\ChannelManager\AudioReader.dll");
            config.Set("AudioReader.pdb", @"${srcRoot}\workdir\ChannelManager\AudioReader.pdb");
            config.Set("AvayaVoipChannel.dll", @"${srcRoot}\workdir\ChannelManager\AvayaVoipChannel.dll");
            config.Set("AvayaVoipChannel.pdb", @"${srcRoot}\workdir\ChannelManager\AvayaVoipChannel.pdb");
            config.Set("centricity.dll", @"${srcRoot}\workdir\centricity\ET\bin\centricity.dll");
            config.Set("centricity.pdb", @"${srcRoot}\workdir\centricity\ET\bin\centricity.pdb");
            config.Set("Centricity_BLL.dll", @"${srcRoot}\workdir\Centricity\ET\bin\Centricity_BLL.dll");
            config.Set("Centricity_BLL.pdb", @"${srcRoot}\workdir\Centricity\ET\bin\Centricity_BLL.pdb");
            config.Set("Centricity_BLL.XmlSerializers.dll", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_BLL.XmlSerializers.dll");
            config.Set("Centricity_DAL.dll", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_DAL.dll");
            config.Set("Centricity_DAL.pdb", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_DAL.pdb");
            config.Set("Centricity_deploy.dll", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_deploy.dll");
            config.Set("Centricity_SCA.dll", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_SCA.dll");
            config.Set("Centricity_deploy.resources.dll_DE", @"${srcRoot}\workdir\centricity\ET\bin\de\Centricity_deploy.resources.dll");
            config.Set("Centricity_deploy.resources.dll_DE_1", @"${srcRoot}\workdir\centricity\ET\bin\de-DE\Centricity_deploy.resources.dll");
            config.Set("Centricity_deploy.resources.dll_ES", @"${srcRoot}\workdir\centricity\ET\bin\es\Centricity_deploy.resources.dll");
            config.Set("Centricity_Shared.dll", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_Shared.dll");
            config.Set("Centricity_Shared.pdb", @"${srcRoot}\workdir\centricity\ET\bin\Centricity_Shared.pdb");
            config.Set("Chanmgr_common.xsd", @"${srcRoot}\config\chanmgr\Chanmgr_common.xsd");
            config.Set("ChanMgrSvc.exe", @"${srcRoot}\workdir\ChannelManager\ChanMgrSvc.exe");
            config.Set("ChanMgrSvc.pdb", @"${srcRoot}\workdir\ChannelManager\ChanMgrSvc.pdb");
            config.Set("ChanMgrSvc.SIP.config", @"${srcRoot}\config\chanmgr\ChanMgrSvc.SIP.config");
            config.Set("ChannelBrokerService.xml", @"${srcRoot}\config\server\C2CServiceDescriptions\ChannelBrokerService.xml");
            config.Set("ChannelManager.ICM.xml", @"${srcRoot}\config\chanmgr\ChannelManager.ICM.xml");
            config.Set("ChannelManager.SIP.xml", @"${srcRoot}\config\chanmgr\ChannelManager.SIP.xml");
            config.Set("CiscoICM.dll", @"${srcRoot}\workdir\ContactSourceRunner\CiscoICM.dll");
            config.Set("cleanup-SIPGateway-dir.bat", @"${srcRoot}\config\chanmgr\cleanup-SIPGateway-dir.bat");
            config.Set("client.properties", @"${srcRoot}\config\clients\client.properties");
            config.Set("CommonUpdates.xml", @"${srcRoot}\config\server\DatabaseUpdates\CommonUpdates.xml");
            config.Set("ContactSourceRunner.bat", @"${srcRoot}\config\sourcerunnerservice\ContactSourceRunner.bat");
            config.Set("ContactSources.properties", @"${srcRoot}\config\sourcerunnerservice\ContactSources.properties");
            config.Set("Create_Centricity_WFM_SPROCS.sql", @"${srcRoot}\config\server\Create_Centricity_WFM_SPROCS.sql");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            config.Set("cstaLoader.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader.dll");
            config.Set("cstaLoader.pdb", @"${srcRoot}\src\contactsources\tsapi\cstaLoader\Release\cstaLoader.pdb");

            config.Set("cstaLoader_1_2.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_1_2.dll");
            config.Set("cstaLoader_1_3_3.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_1_3_3.dll");
            config.Set("cstaLoader_3_33.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_3_33.dll");
            config.Set("cstaLoader_6_4_3.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_6_4_3.dll");
            config.Set("cstaLoader_9_1.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_9_1.dll");
            config.Set("cstaLoader_9_5.dll", @"${srcRoot}\workdir\ContactSourceRunner\cstaLoader_9_5.dll");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            // FIXME the names of the files don't match (6.0, 6_0)
            config.Set("ctcLoader_6.0.dll", @"${srcRoot}\workdir\ContactSourceRunner\ctcLoader_6.0.dll");
            config.Set("ctcLoader_6_0.pdb", @"${srcRoot}\src\contactsources\netmerge\ctcLoader_6_0\Release\ctcLoader_6_0.pdb");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            // FIXME the names of the files don't match (7.0, 7_0)
            config.Set("ctcLoader_7.0.dll", @"${srcRoot}\workdir\ContactSourceRunner\ctcLoader_7.0.dll");
            config.Set("ctcLoader_7_0.pdb", @"${srcRoot}\src\contactsources\netmerge\ctcLoader_7_0\Release\ctcLoader_7_0.pdb");

            config.Set("DBMigration_84SP9_To_10.sql", @"${srcRoot}\src\tools\DBMigration\v2\DBMigration_84SP9_To_10.sql");
            config.Set("Default.aspx", @"${srcRoot}\workdir\centricity\ET\Home\Send\Default.aspx");
            config.Set("DefaultEnvisionProfile.prx", @"${srcRoot}\src\winservices\WMWrapperService\DefaultEnvisionProfile.prx");
            config.Set("DemoModeChannel.dll", @"${srcRoot}\workdir\ChannelManager\DemoModeChannel.dll");
            config.Set("DemoModeChannel.pdb", @"${srcRoot}\workdir\ChannelManager\DemoModeChannel.pdb");
            config.Set("DialogicChannel.dll", @"${srcRoot}\workdir\ChannelManager\DialogicChannel.dll");
            config.Set("DialogicChannel.pdb", @"${srcRoot}\workdir\ChannelManager\DialogicChannel.pdb");
            config.Set("DialogicChannel60.dll", @"${srcRoot}\workdir\ChannelManager\DialogicChannel60.dll");
            config.Set("DialogicChannel60.pdb", @"${srcRoot}\workdir\ChannelManager\DialogicChannel60.pdb");
            config.Set("DMCCConfigLib.dll", @"${srcRoot}\workdir\ChannelManager\DMCCConfigLib.dll");
            config.Set("DMCCConfigLib.pdb", @"${srcRoot}\workdir\ChannelManager\DMCCConfigLib.pdb");
            config.Set("dmcc_devices.bat", @"${srcRoot}\src\tools\chanmgr\dmcc_devices.bat");
            config.Set("DMCCWrapperLib.dll", @"${srcRoot}\workdir\ChannelManager\DMCCWrapperLib.dll");
            config.Set("DMCCWrapperLib.pdb", @"${srcRoot}\workdir\ChannelManager\DMCCWrapperLib.pdb");
            config.Set("DMCCWrapperLib.tlb", @"${srcRoot}\workdir\ChannelManager\DMCCWrapperLib.tlb");
            config.Set("EditEvaluation.aspx", @"${srcRoot}\workdir\centricity\ET\PerformanceManagement\Evaluations\EditEvaluation.aspx");
            config.Set("Envision.jar", @"${srcRoot}\Release\Envision.jar");
            // use EN by default, but this needs to be fixed properly
            // caution: don't use @"${srcRoot}\setup\Signature\EnvisionControls.cab"
            config.Set("EnvisionControls.cab", @"${srcRoot}\setup\Signature\EN\EnvisionControls.cab");
            config.Set("EnvisionServer.bat", @"${srcRoot}\config\server\EnvisionServer.bat");
            config.Set("EnvisionServer.exe_1", @"${srcRoot}\workdir\etservice\EnvisionServer.exe");
            config.Set("EnvisionSR.bat", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\EnvisionSR.bat");
            config.Set("EnvisionSR.reg", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\EnvisionSR.reg");
            config.Set("envision_schema.xml", @"${srcRoot}\config\server\envision_schema.xml");
            config.Set("envision_schema_central.xml", @"${srcRoot}\config\server\envision_schema_central.xml");
            config.Set("EnvisionTheme.css", @"${srcRoot}\workdir\centricity\ET\App_Themes\EnvisionTheme\EnvisionTheme.css");
            config.Set("ETContactSource.exe", @"${srcRoot}\workdir\ContactSourceRunner\ETContactSource.exe");
            config.Set("ETContactSource.pdb", @"${srcRoot}\workdir\ContactSourceRunner\ETContactSource.pdb");
            config.Set("ETScheduleService.xml", @"${srcRoot}\config\server\C2CServiceDescriptions\ETScheduleService.xml");
            config.Set("ETService.exe", @"${srcRoot}\workdir\etservice\ETService.exe");
            config.Set("instsrv.exe", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\instsrv.exe");
            config.Set("IPXChannel.dll", @"${srcRoot}\workdir\ChannelManager\IPXChannel.dll");
            config.Set("IPXChannel.pdb", @"${srcRoot}\workdir\ChannelManager\IPXChannel.pdb");
            config.Set("log4net.dll", @"${srcRoot}\workdir\SharedResources\log4net.dll");
            config.Set("JtapiItemService.xml", @"${srcRoot}\config\server\C2CServiceDescriptions\JtapiItemService.xml");
            config.Set("log4j.properties.template", @"${srcRoot}\config\sourcerunnerservice\log4j.properties.template");
            config.Set("LumiSoft.Net.dll", @"${srcRoot}\workdir\SharedResources\LumiSoft.Net.dll");
            config.Set("LumiSoft.Net.pdb", @"${srcRoot}\workdir\SharedResources\LumiSoft.Net.pdb");
            config.Set("LumiSoft.Net.xml", @"${srcRoot}\src\Components\LumiSoft_SIP_SDK\LumiSoft.Net.xml");
            config.Set("manifest.xml", @"${srcRoot}\config\server\ArchitectureServiceDescriptions\manifest.xml");
            config.Set("manifest.xml_1", @"${srcRoot}\config\server\BIServiceDescriptions\manifest.xml");
            config.Set("manifest.xml_2", @"${srcRoot}\config\server\C2CServiceDescriptions\manifest.xml");
            config.Set("manifest.xml_3", @"${srcRoot}\config\server\EWMServiceDescriptions\manifest.xml");
            config.Set("manifest.xml_4", @"${srcRoot}\config\server\IntegrationsServiceDescriptions\manifest.xml");
            config.Set("manifest.xml_5", @"${srcRoot}\config\server\LoggerServiceDescriptions\manifest.xml");
            config.Set("MSSQLUpdate_build_10.0.0303.1.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0303.1.xml");
            config.Set("MSSQLUpdate_build_10.1.2.0.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.2.0.xml");
            config.Set("nativeServiceWin32.dll", @"${srcRoot}\workdir\server\nativeServiceWin32.dll");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            config.Set("NetMerge.dll", @"${srcRoot}\workdir\ContactSourceRunner\NetMerge.dll");
            config.Set("NetMerge.pdb", @"${srcRoot}\src\contactsources\netmerge\Release\NetMerge.pdb");

            config.Set("NewEvaluation.aspx", @"${srcRoot}\workdir\centricity\ET\PerformanceManagement\Evaluations\NewEvaluation.aspx");
            config.Set("RadEditor.skin", @"${srcRoot}\workdir\centricity\ET\App_Themes\EnvisionTheme\RadEditor.skin");
            config.Set("RAL.dll", @"${srcRoot}\workdir\centricity\ET\bin\RAL.dll");
            config.Set("RAL.pdb", @"${srcRoot}\workdir\centricity\ET\bin\RAL.pdb");
            config.Set("RtpTransmitter.dll", @"${srcRoot}\workdir\ChannelManager\RtpTransmitter.dll");
            config.Set("RtpTransmitter.pdb", @"${srcRoot}\workdir\ChannelManager\RtpTransmitter.pdb");
            config.Set("server.dll", @"${srcRoot}\workdir\SharedResources\server.dll");
            config.Set("server.pdb", @"${srcRoot}\workdir\SharedResources\server.pdb");
            config.Set("SIPChannel.dll", @"${srcRoot}\workdir\ChannelManager\SIPChannel.dll");
            config.Set("SIPChannel.pdb", @"${srcRoot}\workdir\ChannelManager\SIPChannel.pdb");
            config.Set("SIPConfigLib.dll", @"${srcRoot}\workdir\ChannelManager\SIPConfigLib.dll");
            config.Set("SIPConfigLib.pdb", @"${srcRoot}\workdir\ChannelManager\SIPConfigLib.pdb");
            config.Set("SIP_events.properties", @"${srcRoot}\config\chanmgr\SIP_events.properties");
            config.Set("SIPPhone.dll", @"${srcRoot}\workdir\ChannelManager\SIPPhone.dll");
            config.Set("SIPPhone.pdb", @"${srcRoot}\workdir\ChannelManager\SIPPhone.pdb");
            config.Set("SIPWrapperLib.dll", @"${srcRoot}\workdir\ChannelManager\SIPWrapperLib.dll");
            config.Set("SIPWrapperLib.pdb", @"${srcRoot}\workdir\ChannelManager\SIPWrapperLib.pdb");
            config.Set("SIPWrapperLib.tlb", @"${srcRoot}\workdir\ChannelManager\SIPWrapperLib.tlb");
            config.Set("SIPWrapperLogging.xml", @"${srcRoot}\config\chanmgr\SIPWrapperLogging.xml");
            config.Set("SiteToGroupAgentMover.ascx", @"${srcRoot}\workdir\centricity\ET\UserControls\Movers\SiteToGroupAgentMover.ascx");
            config.Set("SiteToGroupAgentMover.ascx.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.resx");
            config.Set("SiteToGroupAgentMover.ascx.de.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.de.resx");
            config.Set("SiteToGroupAgentMover.ascx.es.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.es.resx");
            config.Set("sleep.exe", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\sleep.exe");
            config.Set("SourceRunnerService.exe", @"${srcRoot}\workdir\ContactSourceRunner\SourceRunnerService.exe");
            config.Set("SourceRunnerService.pdb", @"${srcRoot}\workdir\ContactSourceRunner\SourceRunnerService.pdb");
            config.Set("srvany.exe", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\srvany.exe");
            config.Set("states.BIB.xml", @"${srcRoot}\config\chanmgr\states.BIB.xml");
            config.Set("svcmgr.exe", @"${srcRoot}\src\tools\Scripts\ChannelManager\EnvisionSR\svcmgr.exe");
            config.Set("TeliaCallGuide.dll", @"${srcRoot}\workdir\ContactSourceRunner\TeliaCallGuide.dll");
            config.Set("TeliaCallGuide.pdb", @"${srcRoot}\workdir\ContactSourceRunner\TeliaCallGuide.pdb");
            config.Set("TokenService.xml", @"${srcRoot}\config\server\ArchitectureServiceDescriptions\TokenService.xml");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            config.Set("Tsapi.dll", @"${srcRoot}\workdir\ContactSourceRunner\Tsapi.dll");
            config.Set("Tsapi.pdb", @"${srcRoot}\src\contactsources\tsapi\Release\Tsapi.pdb");

            //config.Set("web.config", @"${srcRoot}\src\clients\centricity\ET\web.config");
            config.Set("WMWrapperService.exe", @"${srcRoot}\src\winservices\WMWrapperService\bin\Release\WMWrapperService.exe");

            // AVPlayer
            config.Set("AVPlayer.application", @"${srcRoot}\workdir\AVPlayer\AVPlayer.application");
            config.Set("AgentSupport.exe.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\AgentSupport.exe.deploy");
            config.Set("AVPlayer.exe.config.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.config.deploy");
            config.Set("AVPlayer.exe.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.deploy");
            config.Set("AVPlayer.exe.manifest", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.manifest");
            config.Set("CentricityApp.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\CentricityApp.dll.deploy");
            config.Set("hasp_windows.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\hasp_windows.dll.deploy");
            config.Set("Interop.WMPLib.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\Interop.WMPLib.dll.deploy");
            config.Set("log4net.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\log4net.dll.deploy");
            config.Set("nativeServiceWin32.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\nativeServiceWin32.dll.deploy");
            config.Set("server.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\server.dll.deploy");
            config.Set("SharedResources.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\SharedResources.dll.deploy");
            config.Set("ISource.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\_ISource.dll.deploy");
            config.Set("AVPlayer.resources.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\de\AVPlayer.resources.dll.deploy");
            config.Set("AVPlayer.resources.dll.deploy_1", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\es\AVPlayer.resources.dll.deploy");
            config.Set("CentricityApp.resources.dll.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            config.Set("CentricityApp.resources.dll.deploy_1", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            config.Set("AVPlayerIcon.ico.deploy", @"${srcRoot}\workdir\AVPlayer\Application Files\AVPlayer_${webapps_version}\Resources\AVPlayerIcon.ico.deploy");

            // RecordingDownloadTool
            config.Set("RecordingDownloadTool.application", @"${srcRoot}\workdir\RecordingDownloadTool\RecordingDownloadTool.application");
            config.Set("CentricityApp.dll.deploy_1", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\CentricityApp.dll.deploy");
            config.Set("log4net.dll.deploy_1", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\log4net.dll.deploy");
            config.Set("RecordingDownloadTool.exe.config.deploy", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.config.deploy");
            config.Set("RecordingDownloadTool.exe.deploy", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.deploy");
            config.Set("RecordingDownloadTool.exe.manifest", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.manifest");
            config.Set("server.dll.deploy_1", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\server.dll.deploy");
            config.Set("sox.exe.deploy", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\sox.exe.deploy");
            config.Set("CentricityApp.resources.dll.deploy_2", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            config.Set("CentricityApp.resources.dll.deploy_3", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            config.Set("RecordingDownloadTool.resources.dll.deploy", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\de\RecordingDownloadTool.resources.dll.deploy");
            config.Set("RecordingDownloadTool.resources.dll.deploy_1", @"${srcRoot}\workdir\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\es\RecordingDownloadTool.resources.dll.deploy");

            // from %ETSDK%
            try
            {
                string ETSDK = Environment.GetEnvironmentVariable("ETSDK");
                string gacutil = Path.Combine(ETSDK, @"Microsoft.NET\v3.5\gacutil.exe");
                config.Set("gacutil.exe", gacutil);
                string installutil = Path.Combine(ETSDK, @"Microsoft.NET\v2.0\InstallUtil.exe");
                config.Set("InstallUtil.exe", installutil);
                string regasm = Path.Combine(ETSDK, @"Microsoft.NET\v2.0\regasm.exe");
                config.Set("regasm.exe", regasm);
                string sc = Path.Combine(ETSDK, @"Microsoft\sc.exe");
                config.Set("sc.exe", sc);

                string jtapi_jar = Path.Combine(ETSDK, @"cti_libs\jtapi\jtapi.jar");
                config.Set("jtapi.jar", jtapi_jar);
                string jtracing_jar = Path.Combine(ETSDK, @"cti_libs\jtapi\jtracing.jar");
                config.Set("jtracing.jar", jtracing_jar);
                string updater_jar = Path.Combine(ETSDK, @"cti_libs\jtapi\updater.jar");
                config.Set("updater.jar", updater_jar);

                // LAA-BIN
                string dumpbin = Path.Combine(Environment.GetEnvironmentVariable("ETSDK"), @"java\LAA-BIN\dumpbin.exe");
                config.Set("dumpbin.exe", dumpbin);
                string EnvisionServer = Path.Combine(Environment.GetEnvironmentVariable("ETSDK"), @"java\LAA-BIN\EnvisionServer.exe");
                config.Set("EnvisionServer.exe", EnvisionServer);
                string java = Path.Combine(Environment.GetEnvironmentVariable("ETSDK"), @"java\LAA-BIN\java.exe");
                config.Set("java.exe", java);
                string javaw = Path.Combine(Environment.GetEnvironmentVariable("ETSDK"), @"java\LAA-BIN\javaw.exe");
                config.Set("javaw.exe", javaw);
            }
            catch (ArgumentNullException)
            {
                logger.Fatal("Please set %ETSDK% and try again");
            }

            source.ExpandKeyValues();
            source.Save("Aristotle_sources.config");
        }

        // Target config is where the files are installed on each application.
        public void makeTargetConfig(string webapps_version = "0_0_0_0")
        {
            IniConfigSource source = new IniConfigSource();

            // Each patchableApp (see Clyde.cs) needs its own config section and appRoot.
            IConfig server = source.AddConfig("Server");
            server.Set("serverRoot", @".");

            server.Set("Administrator.exe", @"${serverRoot}\Administrator.exe");
            server.Set("AgentAutomation.dll", @"${serverRoot}\AgentAutomation.dll");
            server.Set("AlvasAudio.dll", @"${serverRoot}\AlvasAudio.dll");
            server.Set("AlvasAudio.pdb", @"${serverRoot}\AlvasAudio.pdb");
            server.Set("AlvasAudio.tlb", @"${serverRoot}\AlvasAudio.tlb");
            server.Set("App_Code.compiled", @"${serverRoot}\bin\App_Code.compiled");
            server.Set("App_global.asax.compiled", @"${serverRoot}\bin\App_global.asax.compiled");
            server.Set("App_GlobalResources.compiled", @"${serverRoot}\bin\App_GlobalResources.compiled");
            server.Set("centricity.dll", @"${serverRoot}\bin\centricity.dll");
            server.Set("centricity.pdb", @"${serverRoot}\bin\centricity.pdb");
            server.Set("Centricity_BLL.dll", @"${serverRoot}\bin\Centricity_BLL.dll");
            server.Set("Centricity_BLL.pdb", @"${serverRoot}\bin\Centricity_BLL.pdb");
            server.Set("Centricity_BLL.XmlSerializers.dll", @"${serverRoot}\bin\Centricity_BLL.XmlSerializers.dll");
            server.Set("Centricity_DAL.dll", @"${serverRoot}\bin\Centricity_DAL.dll");
            server.Set("Centricity_DAL.pdb", @"${serverRoot}\bin\Centricity_DAL.pdb");
            server.Set("Centricity_deploy.resources.dll_DE", @"${serverRoot}\bin\de\Centricity_deploy.resources.dll");
            server.Set("Centricity_deploy.resources.dll_DE_1", @"${serverRoot}\bin\de-DE\Centricity_deploy.resources.dll");
            server.Set("Centricity_deploy.resources.dll_ES", @"${serverRoot}\bin\es\Centricity_deploy.resources.dll");
            server.Set("Centricity_deploy.dll", @"${serverRoot}\bin\Centricity_deploy.dll");
            server.Set("Centricity_SCA.dll", @"${serverRoot}\bin\Centricity_SCA.dll");
            server.Set("Centricity_Shared.dll", @"${serverRoot}\bin\Centricity_Shared.dll");
            server.Set("Centricity_Shared.pdb", @"${serverRoot}\bin\Centricity_Shared.pdb");
            server.Set("ChannelBrokerService.xml", @"${serverRoot}\C2CServiceDescriptions\ChannelBrokerService.xml");
            server.Set("CiscoICM.dll", @"${serverRoot}\ContactSourceRunner\CiscoICM.dll");
            server.Set("CommonUpdates.xml", @"${serverRoot}\DatabaseUpdates\CommonUpdates.xml");
            server.Set("ContactSourceRunner.bat", @"${serverRoot}\ContactSourceRunner\ContactSourceRunner.bat");
            server.Set("ContactSources.properties", @"${serverRoot}\ContactSourceRunner\ContactSources.properties");
            server.Set("Create_Centricity_WFM_SPROCS.sql", @"${serverRoot}\Create_Centricity_WFM_SPROCS.sql");
            server.Set("cstaLoader.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader.dll");
            server.Set("cstaLoader.pdb", @"${serverRoot}\ContactSourceRunner\cstaLoader.pdb");
            server.Set("cstaLoader_1_2.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_1_2.dll");
            server.Set("cstaLoader_1_3_3.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_1_3_3.dll");
            server.Set("cstaLoader_3_33.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_3_33.dll");
            server.Set("cstaLoader_6_4_3.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_6_4_3.dll");
            server.Set("cstaLoader_9_1.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_9_1.dll");
            server.Set("cstaLoader_9_5.dll", @"${serverRoot}\ContactSourceRunner\cstaLoader_9_5.dll");
            // FIXME the names of the files don't match (6.0, 6_0)
            server.Set("ctcLoader_6.0.dll", @"${serverRoot}\ContactSourceRunner\ctcLoader_6.0.dll");
            server.Set("ctcLoader_6_0.pdb", @"${serverRoot}\ContactSourceRunner\ctcLoader_6_0.pdb");
            // FIXME the names of the files don't match (7.0, 7_0)
            server.Set("ctcLoader_7.0.dll", @"${serverRoot}\ContactSourceRunner\ctcLoader_7.0.dll");
            server.Set("ctcLoader_7_0.pdb", @"${serverRoot}\ContactSourceRunner\ctcLoader_7_0.pdb");
            server.Set("client.properties", @"${serverRoot}\client.properties");
            server.Set("Default.aspx", @"${serverRoot}\Home\Send\Default.aspx");
            server.Set("EditEvaluation.aspx", @"${serverRoot}\PerformanceManagement\Evaluations\EditEvaluation.aspx");
            // Note how we configure multiple copies of the same file on the same app
            server.Set("Envision.jar", @"${serverRoot}\Envision.jar|${serverRoot}\WebServer\webapps\ET\WEB-INF\lib\Envision.jar|${serverRoot}\wwwroot\EnvisionComponents\Envision.jar");
            server.Set("envision_schema.xml", @"${serverRoot}\envision_schema.xml");
            server.Set("envision_schema_central.xml", @"${serverRoot}\envision_schema_central.xml");
            server.Set("EnvisionControls.cab", @"${serverRoot}\WebServer\webapps\ET\ETReporting\EnvisionControls.cab");
            server.Set("EnvisionServer.bat", @"${serverRoot}\EnvisionServer.bat");
            server.Set("EnvisionServer.exe_1", @"${serverRoot}\EnvisionServer.exe");
            server.Set("EnvisionTheme.css", @"${serverRoot}\App_Themes\EnvisionTheme\EnvisionTheme.css");
            server.Set("ETContactSource.exe", @"${serverRoot}\ContactSourceRunner\ETContactSource.exe");
            server.Set("ETContactSource.pdb", @"${serverRoot}\ContactSourceRunner\ETContactSource.pdb");
            server.Set("ETScheduleService.xml", @"${serverRoot}\C2CServiceDescriptions\ETScheduleService.xml");
            server.Set("ETService.exe", @"${serverRoot}\ETService.exe");
            server.Set("jtapi.jar", @"${serverRoot}\JRE\lib\ext\jtapi.jar");
            server.Set("JtapiItemService.xml", @"${serverRoot}\C2CServiceDescriptions\JtapiItemService.xml");
            server.Set("jtracing.jar", @"${serverRoot}\JRE\lib\ext\jtracing.jar");
            server.Set("log4j.properties.template", @"${serverRoot}\ContactSourceRunner\log4j.properties.template");
            server.Set("manifest.xml", @"${serverRoot}\ArchitectureServiceDescriptions\manifest.xml");
            server.Set("manifest.xml_2", @"${serverRoot}\C2CServiceDescriptions\manifest.xml");
            server.Set("MSSQLUpdate_build_10.0.0303.1.xml", @"${serverRoot}\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0303.1.xml");
            server.Set("MSSQLUpdate_build_10.1.2.0.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.2.0.xml");
            server.Set("nativeServiceWin32.dll", @"${serverRoot}\nativeServiceWin32.dll|${serverRoot}\ContactSourceRunner\nativeServiceWin32.dll");
            server.Set("NetMerge.dll", @"${serverRoot}\ContactSourceRunner\NetMerge.dll");
            server.Set("NetMerge.pdb", @"${serverRoot}\ContactSourceRunner\NetMerge.pdb");
            server.Set("NewEvaluation.aspx", @"${serverRoot}\PerformanceManagement\Evaluations\NewEvaluation.aspx");
            server.Set("RadEditor.skin", @"${serverRoot}\App_Themes\EnvisionTheme\RadEditor.skin");
            server.Set("RAL.dll", @"${serverRoot}\bin\RAL.dll");
            server.Set("RAL.pdb", @"${serverRoot}\bin\RAL.pdb");
            server.Set("server.dll", @"${serverRoot}\bin\server.dll");
            server.Set("server.pdb", @"${serverRoot}\bin\server.pdb");
            server.Set("SIP_events.properties", @"${serverRoot}\ChannelManager\SIP_events.properties");
            server.Set("SiteToGroupAgentMover.ascx", @"${serverRoot}\UserControls\Movers\SiteToGroupAgentMover.ascx");
            server.Set("SiteToGroupAgentMover.ascx.resx", @"${serverRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.resx");
            server.Set("SiteToGroupAgentMover.ascx.de.resx", @"${serverRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.de.resx");
            server.Set("SiteToGroupAgentMover.ascx.es.resx", @"${serverRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.es.resx");
            server.Set("SourceRunnerService.exe", @"${serverRoot}\ContactSourceRunner\SourceRunnerService.exe");
            server.Set("SourceRunnerService.pdb", @"${serverRoot}\ContactSourceRunner\SourceRunnerService.pdb");
            server.Set("TeliaCallGuide.dll", @"${serverRoot}\ContactSourceRunner\TeliaCallGuide.dll");
            server.Set("TeliaCallGuide.pdb", @"${serverRoot}\ContactSourceRunner\TeliaCallGuide.pdb");
            server.Set("TokenService.xml", @"${serverRoot}\ArchitectureServiceDescriptions\TokenService.xml");
            server.Set("Tsapi.dll", @"${serverRoot}\ContactSourceRunner\Tsapi.dll");
            server.Set("Tsapi.pdb", @"${serverRoot}\ContactSourceRunner\Tsapi.pdb");
            server.Set("updater.jar", @"${serverRoot}\JRE\lib\ext\updater.jar");
            //server.Set("web.config", @"${serverRoot}\web.config");

            // LAA-BIN
            server.Set("dumpbin.exe", @"${serverRoot}\LAA-BIN\dumpbin.exe");
            server.Set("EnvisionServer.exe", @"${serverRoot}\LAA-BIN\EnvisionServer.exe");
            server.Set("java.exe", @"${serverRoot}\LAA-BIN\java.exe");
            server.Set("javaw.exe", @"${serverRoot}\LAA-BIN\javaw.exe");

            // AlvasAudio
            server.Set("AlvasAudio.bat", @"${serverRoot}\AlvasAudio\AlvasAudio.bat");
            server.Set("AlvasAudio.dll", @"${serverRoot}\AlvasAudio\AlvasAudio.dll");
            server.Set("gacutil.exe", @"${serverRoot}\AlvasAudio\gacutil.exe");
            server.Set("regasm.exe", @"${serverRoot}\AlvasAudio\regasm.exe");

            IConfig cm = source.AddConfig("ChannelManager");
            cm.Set("cmRoot", @".");

            cm.Set("AlvasAudio.bat", @"${cmRoot}\AlvasAudio\AlvasAudio.bat");
            cm.Set("AlvasAudio.dll", @"${cmRoot}\AlvasAudio\AlvasAudio.dll");
            cm.Set("AlvasAudio.pdb", @"${cmRoot}\AlvasAudio.pdb");
            cm.Set("AlvasAudio.tlb", @"${cmRoot}\AlvasAudio.tlb");
            cm.Set("audiocodesChannel.dll", @"${cmRoot}\audiocodesChannel.dll");
            cm.Set("audiocodesChannel.pdb", @"${cmRoot}\audiocodesChannel.pdb");
            cm.Set("AudioReader.dll", @"${cmRoot}\AudioReader.dll");
            cm.Set("AudioReader.pdb", @"${cmRoot}\AudioReader.pdb");
            cm.Set("AvayaVoipChannel.dll", @"${cmRoot}\AvayaVoipChannel.dll");
            cm.Set("AvayaVoipChannel.pdb", @"${cmRoot}\AvayaVoipChannel.pdb");
            cm.Set("Chanmgr_common.xsd", @"${cmRoot}\Chanmgr_common.xsd");
            cm.Set("ChanMgrSvc.exe", @"${cmRoot}\ChanMgrSvc.exe");
            cm.Set("ChanMgrSvc.pdb", @"${cmRoot}\ChanMgrSvc.pdb");
            cm.Set("ChanMgrSvc.SIP.config", @"${cmRoot}\ChanMgrSvc.SIP.config");
            cm.Set("ChannelManager.ICM.xml", @"${cmRoot}\ChannelManager.ICM.xml");
            cm.Set("ChannelManager.SIP.xml", @"${cmRoot}\ChannelManager.SIP.xml");
            cm.Set("cleanup-SIPGateway-dir.bat", @"${cmRoot}\cleanup-SIPGateway-dir.bat");
            cm.Set("DemoModeChannel.dll", @"${cmRoot}\DemoModeChannel.dll");
            cm.Set("DemoModeChannel.pdb", @"${cmRoot}\DemoModeChannel.pdb");
            cm.Set("DialogicChannel.dll", @"${cmRoot}\DialogicChannel.dll");
            cm.Set("DialogicChannel.pdb", @"${cmRoot}\DialogicChannel.pdb");
            cm.Set("DialogicChannel60.dll", @"${cmRoot}\DialogicChannel60.dll");
            cm.Set("DialogicChannel60.pdb", @"${cmRoot}\DialogicChannel60.pdb");
            cm.Set("DMCCConfigLib.dll", @"${cmRoot}\DMCCConfigLib.dll");
            cm.Set("DMCCConfigLib.pdb", @"${cmRoot}\DMCCConfigLib.pdb");
            cm.Set("dmcc_devices.bat", @"${cmRoot}\dmcc_devices.bat");
            cm.Set("DMCCWrapperLib.dll", @"${cmRoot}\DMCCWrapperLib.dll");
            cm.Set("DMCCWrapperLib.pdb", @"${cmRoot}\DMCCWrapperLib.pdb");
            cm.Set("DMCCWrapperLib.tlb", @"${cmRoot}\DMCCWrapperLib.tlb");
            cm.Set("gacutil.exe", @"${cmRoot}\AlvasAudio\gacutil.exe");
            cm.Set("IPXChannel.dll", @"${cmRoot}\IPXChannel.dll");
            cm.Set("IPXChannel.pdb", @"${cmRoot}\IPXChannel.pdb");
            cm.Set("regasm.exe", @"${cmRoot}\AlvasAudio\regasm.exe");
            cm.Set("RtpTransmitter.dll", @"${cmRoot}\RtpTransmitter.dll");
            cm.Set("RtpTransmitter.pdb", @"${cmRoot}\RtpTransmitter.pdb");
            cm.Set("server.dll", @"${cmRoot}\server.dll");
            cm.Set("server.pdb", @"${cmRoot}\server.pdb");
            cm.Set("SIPChannel.dll", @"${cmRoot}\SIPChannel.dll");
            cm.Set("SIPChannel.pdb", @"${cmRoot}\SIPChannel.pdb");
            cm.Set("SIPConfigLib.dll", @"${cmRoot}\SIPConfigLib.dll");
            cm.Set("SIPConfigLib.pdb", @"${cmRoot}\SIPConfigLib.pdb");
            cm.Set("SIPPhone.dll", @"${cmRoot}\SIPPhone.dll");
            cm.Set("SIPPhone.pdb", @"${cmRoot}\SIPPhone.pdb");
            cm.Set("SIPWrapperLib.dll", @"${cmRoot}\SIPWrapperLib.dll");
            cm.Set("SIPWrapperLib.pdb", @"${cmRoot}\SIPWrapperLib.pdb");
            cm.Set("SIPWrapperLib.tlb", @"${cmRoot}\SIPWrapperLib.tlb");
            cm.Set("SIPWrapperLogging.xml", @"${cmRoot}\SIPWrapperLogging.xml");
            cm.Set("SIP_events.properties", @"${cmRoot}\SIP_events.properties");
            cm.Set("states.BIB.xml", @"${cmRoot}\states.BIB.xml");

            // EnvisionSR
            cm.Set("EnvisionSR.bat", @"${cmRoot}\EnvisionSR\EnvisionSR.bat");
            cm.Set("EnvisionSR.reg", @"${cmRoot}\EnvisionSR\EnvisionSR.reg");
            cm.Set("instsrv.exe", @"${cmRoot}\EnvisionSR\instsrv.exe");
            cm.Set("sleep.exe", @"${cmRoot}\EnvisionSR\sleep.exe");
            cm.Set("srvany.exe", @"${cmRoot}\EnvisionSR\srvany.exe");
            cm.Set("svcmgr.exe", @"${cmRoot}\EnvisionSR\svcmgr.exe");
            cm.Set("LumiSoft.Net.dll", @"${cmRoot}\LumiSoft.Net.dll");
            cm.Set("LumiSoft.Net.pdb", @"${cmRoot}\LumiSoft.Net.pdb");
            cm.Set("LumiSoft.Net.xml", @"${cmRoot}\LumiSoft.Net.xml");

            // AlvasAudio
            cm.Set("AlvasAudio.bat", @"${cmRoot}\AlvasAudio\AlvasAudio.bat");
            cm.Set("AlvasAudio.dll", @"${cmRoot}\AlvasAudio\AlvasAudio.dll");
            cm.Set("gacutil.exe", @"${cmRoot}\AlvasAudio\gacutil.exe");
            cm.Set("regasm.exe", @"${cmRoot}\AlvasAudio\regasm.exe");


            IConfig wmws = source.AddConfig("WMWrapperService");
            wmws.Set("wmwsRoot", @".");
            wmws.Set("DefaultEnvisionProfile.prx", @"${wmwsRoot}\DefaultEnvisionProfile.prx");
            wmws.Set("server.dll", @"${wmwsRoot}\server.dll");
            wmws.Set("WMWrapperService.exe", @"${wmwsRoot}\WMWrapperService.exe");


            IConfig dbmigration = source.AddConfig("DBMigration");
            dbmigration.Set("dbmigrationRoot", @".");
            dbmigration.Set("DBMigration_84SP9_To_10.sql", @"${dbmigrationRoot}\DBMigration_84SP9_To_10.sql");


            IConfig avplayer = source.AddConfig("AVPlayer");
            avplayer.Set("avplayerRoot", @".");
            avplayer.Set("webapps_version", webapps_version);
            // shows up in patchFiles as "...\patchFiles\AVPlayer\AVPlayer"
            // where the first AVPlayer is the application name, and the
            // second AVPlayer is the subdir on disk
            avplayer.Set("AVPlayer.application", @"${avplayerRoot}\AVPlayer\AVPlayer.application");
            avplayer.Set("AgentSupport.exe.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\AgentSupport.exe.deploy");
            avplayer.Set("AVPlayer.exe.config.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.config.deploy");
            avplayer.Set("AVPlayer.exe.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.deploy");
            avplayer.Set("AVPlayer.exe.manifest", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.manifest");
            avplayer.Set("CentricityApp.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\CentricityApp.dll.deploy");
            avplayer.Set("hasp_windows.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\hasp_windows.dll.deploy");
            avplayer.Set("Interop.WMPLib.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\Interop.WMPLib.dll.deploy");
            avplayer.Set("log4net.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\log4net.dll.deploy");
            avplayer.Set("nativeServiceWin32.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\nativeServiceWin32.dll.deploy");
            avplayer.Set("server.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\server.dll.deploy");
            avplayer.Set("SharedResources.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\SharedResources.dll.deploy");
            avplayer.Set("ISource.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\_ISource.dll.deploy");
            avplayer.Set("AVPlayer.resources.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\de\AVPlayer.resources.dll.deploy");
            avplayer.Set("AVPlayer.resources.dll.deploy_1", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\es\AVPlayer.resources.dll.deploy");
            avplayer.Set("CentricityApp.resources.dll.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            avplayer.Set("CentricityApp.resources.dll.deploy_1", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            avplayer.Set("AVPlayerIcon.ico.deploy", @"${avplayerRoot}\AVPlayer\Application Files\AVPlayer_${webapps_version}\Resources\AVPlayerIcon.ico.deploy");


            IConfig rdtool = source.AddConfig("RecordingDownloadTool");
            rdtool.Set("rdtoolRoot", @".");
            rdtool.Set("webapps_version", webapps_version);
            rdtool.Set("RecordingDownloadTool.application", @"${rdtoolRoot}\RecordingDownloadTool\RecordingDownloadTool.application");
            rdtool.Set("CentricityApp.dll.deploy_1", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\CentricityApp.dll.deploy");
            rdtool.Set("log4net.dll.deploy_1", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\log4net.dll.deploy");
            rdtool.Set("RecordingDownloadTool.exe.config.deploy", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.config.deploy");
            rdtool.Set("RecordingDownloadTool.exe.deploy", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.deploy");
            rdtool.Set("RecordingDownloadTool.exe.manifest", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.manifest");
            rdtool.Set("server.dll.deploy_1", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\server.dll.deploy");
            rdtool.Set("sox.exe.deploy", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\sox.exe.deploy");
            rdtool.Set("CentricityApp.resources.dll.deploy_2", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            rdtool.Set("CentricityApp.resources.dll.deploy_3", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            rdtool.Set("RecordingDownloadTool.resources.dll.deploy", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\de\RecordingDownloadTool.resources.dll.deploy");
            rdtool.Set("RecordingDownloadTool.resources.dll.deploy_1", @"${rdtoolRoot}\RecordingDownloadTool\Application Files\RecordingDownloadTool_${webapps_version}\es\RecordingDownloadTool.resources.dll.deploy");

            source.ExpandKeyValues();
            source.Save("Aristotle_targets.config");
        }

        // Each application passes in a list of keys that identifies files to patch.  Walk over the list and copy each
        // source file to it's destination.
        //
        // Depends: appKeys, sourceConfig and targetConfig all have to use the same key names.
        public void makePortablePatch(string appToPatch, IEnumerable<string> appKeys)
        {
            // Make the roots.  Sometimes they'll be empty--fix it later.
            DirectoryInfo di = new DirectoryInfo(Path.Combine(SourceDir, appToPatch));
            if (di.Exists == false)
            {
                di.Create();
            }
            else
            {
                logger.Error("directory already exists: {0}", di.FullName);
                Environment.Exit(1);
            }

            IConfigSource sourceConfig = new IniConfigSource("Aristotle_sources.config");
            IConfigSource targetConfig = new IniConfigSource("Aristotle_targets.config");
            foreach (string key in appKeys)
            {
                // Try all patchableApps, but skip if their appKeys are empty.  This lets me update the patch lists
                // in PacMan.cs without messing around in here.  Alternate approach is try to fetch the key, log
                // warning when not found.
                if (key.Length == 0)
                {
                    break;
                }
                else
                {
                    string source = sourceConfig.Configs["Sources"].Get(key);
                    string[] targets = targetConfig.Configs[appToPatch].Get(key).Split('|');

                    foreach (string t in targets)
                    {
                        // full paths to each file being added to the patch
                        string fqTargetPath = Path.GetFullPath(Path.Combine(di.ToString(), t));

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fqTargetPath));
                            File.Copy(source, fqTargetPath);
                        }
                        catch (Exception e)
                        {
                            if (e is FileNotFoundException || e is DirectoryNotFoundException)
                            {
                                // this should fail with logger.Fatal() if a needed file is not found
                                logger.Error("not found: {0}", source);
                            }
                        }
                    }
                }
            }
        }

        // Only one format right now.
        public static string formatVersionString(string toFormat)
        {
            // this should match anything from 0.0.0.0 on up
            Regex regex = new Regex(@"\d+(\.)\d+(\.)\d+(\.)\d+");
            string replaced = toFormat;

            // e.g., 10.1.10.92 -> 10_1_10_92
            if (regex.IsMatch(toFormat))
            {
                // probably better to (eventually) configure the logger to send to log only, not stdout too
                //logger.Info("match: {0}", toFormat);
                replaced = Archiver.dotToDash(regex.Match(toFormat));
                //logger.Info("dotToDash: {0}", replaced);
            }
            else
            {
                logger.Info("no match: {0}", toFormat);
            }
            return replaced;
        }

        static string dotToDash(Match m)
        {
            string x = m.ToString();
            return x.Replace(@".", @"_");
        }

        public void run()
        {
            logger.Info("Making archive");

            using (ZipFile zip = new ZipFile())
            {
                if (Directory.Exists(SourceDir))
                {
                    zip.AddDirectory(SourceDir, Path.GetFileName(SourceDir));
                }
                else
                {
                    logger.Fatal("{0} is not a valid directory", SourceDir);
                    Environment.Exit(1);
                }

                // these files install and log the patch
                zip.AddFile("Clyde.exe");
                zip.AddFile("PatchLib.dll");
                zip.AddFile("Nini.dll");
                zip.AddFile("NLog.dll");
                zip.AddFile("NLog.config");

                SelfExtractorSaveOptions options = new SelfExtractorSaveOptions();
                options.Flavor = SelfExtractorFlavor.ConsoleApplication;
                options.ProductVersion = VersionInfo.PRODUCT_VERSION;
                options.DefaultExtractDirectory = ExtractDir;
                options.Copyright = VersionInfo.COPYRIGHT;
                options.PostExtractCommandLine = "Clyde.exe";
                // false for dev, (maybe) true for production
                options.RemoveUnpackedFilesAfterExecute = false;

                string patchName = @"envision-installer-" + PatchVersion + @".exe";
                zip.SaveSelfExtractor(patchName, options);
            }
            logger.Info("... done");
        }
    }

    public class InstallerSuite
    {
        public InstallerSuite()
        {
            this.installers = new List<Installer>();
        }

        public string name { get; set; }

        private int _count = 0;
        public int count
        {
            get
            {
                // there's got to be a nice LINQ'y way to do this...
                foreach (Installer i in this.installers)
                {
                    if (i.isInstalled)
                    {
                        _count++;
                    }
                }
                return _count;
            }
            set { _count = value; }
        }

        public List<Installer> installers { get; set; }
    }

    public class Installer
    {
        public Installer()
        {
            this.applications = new List<ETApplication>();
        }

        public Installer(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
            this.applications = new List<ETApplication>();
        }

        public string name { get; set; }
        public string displayName { get; set; }
        public string installLocation { get; set; }
        public string displayVersion { get; set; }
        public List<ETApplication> applications { get; set; }
        public bool isInstalled { get; set; }
    }

    public class ETApplication
    {
        public ETApplication(string name, string displayName, string patchTo = null)
        {
            this.name = name;
            this.displayName = displayName;
            this.installLocation = null;
            // basically it's CombinePaths(e.ExtractDir, e.SourceDir, this.name)
            this.patchFrom = null;
            this.patchTo = patchTo;
        }

        public string name { get; set; }
        public string displayName { get; set; }
        public string installLocation { get; set; }
        public string patchFrom { get; set; }
        public string patchTo { get; set; }
        public bool replaceAll { get; set; }
    }

    public class Extractor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Extractor(string backupFiles = "backupFiles", string patchFiles = "patchFiles")
        {
            Console.SetWindowSize(160, 50);
            this.BackupDir = Path.Combine(ExtractDir, backupFiles);
            this.PatchDir = Path.Combine(ExtractDir, patchFiles);
        }

        // This should be equivalent to ExtractDir in Archiver.  I should probably find a better solution.
        private string _extractDir = Directory.GetCurrentDirectory();
        public string ExtractDir
        {
            get { return _extractDir; }
        }

        private string _patchDir;
        public string PatchDir
        {
            get { return _patchDir; }
            set { _patchDir = value; }
        }

        private string _backupDir;
        public string BackupDir
        {
            get { return _backupDir; }
            set { _backupDir = value; }
        }

        private string _patchVersion = VersionInfo.PRODUCT_VERSION;
        public string PatchVersion
        {
            get { return _patchVersion; }
        }

        // finally: do the actual work
        public void run(ETApplication app)
        {
            // backup files to patch
            string backupFrom = app.patchTo;
            string backupTo = CombinePaths(this.BackupDir, app.name);

            if (app.replaceAll == true)
            {
                CopyFolder(backupFrom, backupTo);
                logger.Info("got here? (backed up old)");
                Directory.Delete(backupFrom, true);
                logger.Info("got here? (deleted backupFrom (AKA patchTo))");
                CreateDir(app.patchTo);

                // NO-OP copy new files to backup location

                // this makes me throw up a little
                if (new DirectoryInfo(app.patchFrom).Name == new DirectoryInfo(app.patchTo).Name)
                {
                    app.patchTo = Directory.GetParent(app.patchTo).ToString();
                }
                CopyFolder(app.patchFrom, app.patchTo);
                logger.Info("got here? (copied new to patchTo)");
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(app.patchFrom);
                FileInfo[] srcFiles = di.GetFiles("*", SearchOption.AllDirectories);

                // each file in the patch, with relative directories; base paths are the heads
                string tail;
                // each file bound for the old/ directory
                string bakFileOld;

                //
                // backup original files
                //
                foreach (FileInfo f in srcFiles)
                {
                    tail = RelativePath(backupTo, f.FullName);
                    bakFileOld = Path.GetFullPath(Path.Combine(backupFrom, tail));

                    // Create any nested subdirectories included in the patch.  Note, this will loop
                    // over the same location multiple times; it's a little big ugly
                    DirectoryInfo backupSubdirOld = new DirectoryInfo(Path.GetDirectoryName(bakFileOld.ToString()));
                    if (!Directory.Exists(backupSubdirOld.ToString()))
                    {
                        Directory.CreateDirectory(backupSubdirOld.ToString());
                    }

                    try
                    {
                        File.Copy(bakFileOld, backupFrom);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        Console.WriteLine("WARN: a file to backup was not found: {0}", bakFileOld);
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        // This exception occurs when the patch includes a new directory that is not
                        // on the machine being patched.  As a result, the directory is also not in
                        // patches/VERSION/old, which causes this exception.  Ignore it.
                    }
                }

                //
                // patch the application
                //
                CopyFolder(app.patchFrom, app.patchTo);
            }
        }

        public bool CreateDir(string target)
        {
            try
            {
                Directory.CreateDirectory(target.ToString());
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("PatchTool must be run as Administrator on this system", "sorry Charlie");
                return false;
            }
        }

        public void GetInstallInfo(Installer installer)
        {
            string keyName;
            RegistryKey key;

            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.CurrentUser.OpenSubKey(keyName);
            GetRegistryValues(installer, key);
            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(keyName);
            GetRegistryValues(installer, key);
            keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(keyName);
            GetRegistryValues(installer, key);
        }

        private void GetRegistryValues(Installer installer, RegistryKey key)
        {
            try
            {
                foreach (String a in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(a);
                    try
                    {
                        if (subkey.GetValue("DisplayName").ToString() == installer.displayName)
                        {
                            installer.installLocation = subkey.GetValue("InstallLocation").ToString();
                            installer.displayVersion = subkey.GetValue("DisplayVersion").ToString();
                        }
                    }
                    catch (NullReferenceException) { }
                }
            }
            catch (NullReferenceException) { }
        }

        // TC: probably want to return bool and not write to STDOUT
        private void FileCompare(string fileName1, string fileName2, string fileName3)
        {
            try
            {
                FileEquals(fileName1, fileName2);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is DirectoryNotFoundException)
                {
                    logger.Warn("a file to compare was not found: {0}", fileName2);
                    return;
                }
            }

            if (FileEquals(fileName1, fileName2))
            {
                Console.Write("{0, -90}", "* " + fileName3, Console.WindowWidth, Console.WindowHeight);
                //Console.Write("{0, -130}", fileName3, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(String.Format("{0, 9}", "[matches]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
            }
            else
            {
                Console.Write("{0, -90}", "* " + fileName3, Console.WindowWidth, Console.WindowHeight);
                //Console.Write("{0, -130}", fileName3, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0, 9}", "[nomatch]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
            }
        }

        private void FileStat(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                Console.Write("{0, -130}", fileInfo, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(String.Format("{0, 9}", "[present]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
            }
            else
            {
                Console.Write("{0, -130}", fileInfo, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(String.Format("{0, 9}", "[missing]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
            }
        }

        // based on http://stackoverflow.com/questions/968935/c-binary-file-compare
        static bool FileEquals(string fileName1, string fileName2)
        {
            try
            {
                // Check the file size and CRC equality here.. if they are equal...
                using (var file1 = new FileStream(fileName1, FileMode.Open))
                using (var file2 = new FileStream(fileName2, FileMode.Open))
                    return StreamsContentsAreEqual(file1, file2);
            }
            catch (IOException ex)
            {
                // details to the log, summary to the user
                string caption = "Caught IOException";
                string summary;
                summary = "It looks like an Envision server process is still running.";
                summary += " Clyde's command window or the log will have more details.";
                summary += " You can leave this dialog open, go take care of it, then continue.";
                summary += " Or you can cancel.\n\nContinue?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                logger.Error(ex.ToString());
                result = MessageBox.Show(summary, caption, buttons);

                // is this asking for trouble? ;)
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    FileEquals(fileName1, fileName2);
                }
                else
                {
                    throw;
                }
            }
            // this smells
            return false;
        }

        private static bool StreamsContentsAreEqual(Stream stream1, Stream stream2)
        {
            const int bufferSize = 2048 * 2;
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                {
                    return false;
                }

                if (count1 == 0)
                {
                    return true;
                }

                int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }

        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                try
                {
                    File.Copy(file, dest, true);
                }
                catch (FileNotFoundException)
                {
                    logger.Warn("a file to replace was not found: {0}", file);
                }
                catch (IOException ex)
                {
                    // details to the log, summary to the user
                    string caption = "Caught IOException";
                    string summary;
                    summary = "It looks like an Envision server process is still running.";
                    summary += " Clyde's command window or the log will have more details.";
                    summary += " You can leave this dialog open, go take care of it, then continue.";
                    summary += " Or you can cancel.\n\nContinue?";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;

                    logger.Error(ex.ToString());
                    result = MessageBox.Show(summary, caption, buttons);

                    // is this asking for trouble? ;)
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        // can't call CopyFolder(file, dest) because we've got a file, not a directory.
                        try
                        {
                            File.Copy(file, dest, true);
                        }
                        catch (IOException)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        // http://mrpmorris.blogspot.com/2007/05/convert-absolute-path-to-relative-path.html
        private string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

        // http://stackoverflow.com/questions/144439/building-a-directory-string-from-component-parts-in-c
        string CombinePaths(params string[] parts)
        {
            string result = String.Empty;
            foreach (string s in parts)
            {
                result = Path.Combine(result, s);
            }
            return result;
        }
    }
}
