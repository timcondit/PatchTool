using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using com.et.versioninfo;
using Ionic.Zip;
using Microsoft.Win32;
using Nini.Config;
using NLog;

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
    public static class ExitEarly
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void exit(string msg)
        {
            logger.Fatal(msg);
            Console.Write("Press ENTER to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }

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

        // Source config is a list of where to find files in the working copy.  It is independent of the apps to
        // patch.  Every file in makeTargetConfig (identified by key) must be here.
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

            config.Set("Agents.aspx", @"${srcRoot}\workdir\centricity\ET\Home\Agents\Agents.aspx");
            config.Set("Agents.aspx.resx", @"${srcRoot}\workdir\centricity\ET\Home\Agents\App_LocalResources\Agents.aspx.resx");
            config.Set("Agents.aspx.de.resx", @"${srcRoot}\workdir\centricity\ET\Home\Agents\App_LocalResources\Agents.aspx.de.resx");
            config.Set("Agents.aspx.es.resx", @"${srcRoot}\workdir\centricity\ET\Home\Agents\App_LocalResources\Agents.aspx.es.resx");
            config.Set("Recognitions.aspx", @"${srcRoot}\src\clients\centricity\ET\PerformanceManagement\Recognitions\Recognitions.aspx");
            config.Set("RecognitionDashboardItem.ascx", @"${srcRoot}\src\clients\centricity\ET\UserControls\DashboardControls\Recognition\RecognitionDashboardItem.ascx");
            config.Set("AgentInboxGrid.ascx", @"${srcRoot}\src\clients\centricity\ET\UserControls\Grids\AgentInboxGrid.ascx");
            config.Set("AttachedTrainingClipGrid.ascx", @"${srcRoot}\src\clients\centricity\ET\UserControls\Grids\AttachedTrainingClipGrid.ascx");
            config.Set("EvaluationGrid.ascx", @"${srcRoot}\src\clients\centricity\ET\UserControls\Grids\EvaluationGrid.ascx");
            config.Set("TrainingClipGrid.ascx", @"${srcRoot}\src\clients\centricity\ET\UserControls\Grids\TrainingClipGrid.ascx");

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
            config.Set("CentricityMaster.master.resx", @"${srcRoot}\workdir\centricity\ET\App_LocalResources\CentricityMaster.master.resx");
            config.Set("CentricityMaster.master.de.resx", @"${srcRoot}\workdir\centricity\ET\App_LocalResources\CentricityMaster.master.de.resx");
            config.Set("CentricityMaster.master.es.resx", @"${srcRoot}\workdir\centricity\ET\App_LocalResources\CentricityMaster.master.es.resx");
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
            config.Set("DBServiceCentricityWfm.xml", @"${srcRoot}\config\server\ArchitectureServiceDescriptions\DBServiceCentricityWfm.xml");
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

            // DatabaseUpdates
            config.Set("C2CUpdates.xml", @"${srcRoot}\config\server\DatabaseUpdates\C2CUpdates.xml");
            config.Set("CommonUpdates.xml", @"${srcRoot}\config\server\DatabaseUpdates\CommonUpdates.xml");
            config.Set("EWFMUpdates.xml", @"${srcRoot}\config\server\DatabaseUpdates\EWFMUpdates.xml");
            config.Set("manifest.xml_6", @"${srcRoot}\config\server\DatabaseUpdates\manifest.xml");
            config.Set("SpeechUpdates.xml", @"${srcRoot}\config\server\DatabaseUpdates\SpeechUpdates.xml");
            config.Set("MSSQLUpdate_build_10.0.1.1.xml", @"${srcRoot}\config\server\DatabaseUpdates\C2C\10.0\MSSQLUpdate_build_10.0.1.1.xml");
            config.Set("MSSQLUpdate_build_10.1.0.65.xml", @"${srcRoot}\config\server\DatabaseUpdates\C2C\10.1\MSSQLUpdate_build_10.1.0.65.xml");
            config.Set("MSSQLUpdate_build_10.1.0.201.xml", @"${srcRoot}\config\server\DatabaseUpdates\C2C\10.1\MSSQLUpdate_build_10.1.0.201.xml");
            config.Set("MSSQLUpdate_build_9.12.0.28.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.28.xml");
            config.Set("MSSQLUpdate_build_9.12.0.37.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.37.xml");
            config.Set("MSSQLUpdate_build_9.12.0.38.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.38.xml");
            config.Set("MSSQLUpdate_build_10.0.0.22.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0.22.xml");
            config.Set("MSSQLUpdate_build_10.0.0.31.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0.31.xml");
            config.Set("MSSQLUpdate_build_10.0.0303.1.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0303.1.xml");
            config.Set("MSSQLUpdate_build_10.0.1.1.xml_1", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.1.1.xml");
            config.Set("MSSQLUpdate_build_10.1.0.140.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.140.xml");
            config.Set("MSSQLUpdate_build_10.1.0.151.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.151.xml");
            config.Set("MSSQLUpdate_build_10.1.0.172.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.172.xml");
            config.Set("MSSQLUpdate_build_10.1.0.236a.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.236a.xml");
            config.Set("MSSQLUpdate_build_10.1.0.236b.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.236b.xml");
            config.Set("MSSQLUpdate_build_10.1.0.242.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.242.xml");
            config.Set("MSSQLUpdate_build_10.1.0.333.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.333.xml");
            config.Set("MSSQLUpdate_build_10.1.0.47.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.47.xml");
            config.Set("MSSQLUpdate_build_10.1.0.61.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.61.xml");
            config.Set("MSSQLUpdate_build_10.1.0.62.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.62.xml");
            config.Set("MSSQLUpdate_build_10.1.0.65.xml_1", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.65.xml");
            config.Set("MSSQLUpdate_build_10.1.0.99.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.99.xml");
            config.Set("MSSQLUpdate_build_10.1.2.0.xml", @"${srcRoot}\config\server\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.2.0.xml");

            config.Set("nativeServiceWin32.dll", @"${srcRoot}\workdir\server\nativeServiceWin32.dll");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            config.Set("NetMerge.dll", @"${srcRoot}\workdir\ContactSourceRunner\NetMerge.dll");
            config.Set("NetMerge.pdb", @"${srcRoot}\src\contactsources\netmerge\Release\NetMerge.pdb");

            config.Set("NewEvaluation.aspx", @"${srcRoot}\workdir\centricity\ET\PerformanceManagement\Evaluations\NewEvaluation.aspx");
            config.Set("RadEditor.skin", @"${srcRoot}\workdir\centricity\ET\App_Themes\EnvisionTheme\RadEditor.skin");
            config.Set("RAL.dll", @"${srcRoot}\workdir\centricity\ET\bin\RAL.dll");
            config.Set("RAL.pdb", @"${srcRoot}\workdir\centricity\ET\bin\RAL.pdb");
            config.Set("RecordingGridToolbar.ascx", @"${srcRoot}\workdir\centricity\ET\UserControls\GridToolbar\RecordingGridToolbar.ascx");
            config.Set("RecordingGridToolbar.ascx.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.resx");
            config.Set("RecordingGridToolbar.ascx.de.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.de.resx");
            config.Set("RecordingGridToolbar.ascx.es.resx", @"${srcRoot}\workdir\centricity\ET\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.es.resx");
            config.Set("RtpTransmitter.dll", @"${srcRoot}\workdir\ChannelManager\RtpTransmitter.dll");
            config.Set("RtpTransmitter.pdb", @"${srcRoot}\workdir\ChannelManager\RtpTransmitter.pdb");
            config.Set("server.dll", @"${srcRoot}\workdir\SharedResources\server.dll");
            config.Set("server.pdb", @"${srcRoot}\workdir\SharedResources\server.pdb");
            config.Set("Set_ChannelDeviceIds.sql", @"${srcRoot}\config\chanmgr\SQLScripts\Set_ChannelDeviceIds.sql");
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
            config.Set("Telerik.Web.Design.dll", @"${srcRoot}\workdir\centricity\ET\bin\Telerik.Web.Design.dll");
            config.Set("Telerik.Web.UI.dll", @"${srcRoot}\workdir\centricity\ET\bin\Telerik.Web.UI.dll");
            config.Set("Telerik.Web.UI.xml", @"${srcRoot}\workdir\centricity\ET\bin\Telerik.Web.UI.xml");
            config.Set("TeliaCallGuide.dll", @"${srcRoot}\workdir\ContactSourceRunner\TeliaCallGuide.dll");
            config.Set("TeliaCallGuide.pdb", @"${srcRoot}\workdir\ContactSourceRunner\TeliaCallGuide.pdb");
            config.Set("TokenService.xml", @"${srcRoot}\config\server\ArchitectureServiceDescriptions\TokenService.xml");

            // FIXME these should come from the same place.  Installer and the patch tool should be updated.
            config.Set("Tsapi.dll", @"${srcRoot}\workdir\ContactSourceRunner\Tsapi.dll");
            config.Set("Tsapi.pdb", @"${srcRoot}\src\contactsources\tsapi\Release\Tsapi.pdb");

            //config.Set("web.config", @"${srcRoot}\src\clients\centricity\ET\web.config");
            config.Set("WMWrapperService.exe", @"${srcRoot}\src\winservices\WMWrapperService\bin\Release\WMWrapperService.exe");
            config.Set("WMWrapperService.xml", @"${srcRoot}\config\server\C2CServiceDescriptions\WMWrapperService.xml");

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

            // documentation
            config.Set("Centricity_Webhelp_DE", @"${srcRoot}\documentation\WebHelp\DE\Centricity_Webhelp.zip");
            config.Set("Centricity_Webhelp_EN", @"${srcRoot}\documentation\WebHelp\EN\Centricity_Webhelp.zip");
            config.Set("Centricity_Webhelp_ES", @"${srcRoot}\documentation\WebHelp\ES\Centricity_Webhelp.zip");

            // WFMSG user sync tool
            config.Set("WFMSGUserSync.exe", @"${srcRoot}\src\tools\WFMSGUserSync\WFMSGUserSync\bin\Release\WFMSGUserSync.exe");
            config.Set("WFM_SyncUser.sql", @"${srcRoot}\src\tools\WFMSGUserSync\Sproc\WFM_SyncUser.sql");

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
                ExitEarly.exit("Please set %ETSDK% and try again");
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
            server.Set("ChannelBrokerService.xml", @"${serverRoot}\C2CServiceDescriptions\ChannelBrokerService.xml");
            server.Set("CiscoICM.dll", @"${serverRoot}\ContactSourceRunner\CiscoICM.dll");
            server.Set("ContactSourceRunner.bat", @"${serverRoot}\ContactSourceRunner\ContactSourceRunner.bat");
            server.Set("ContactSources.properties", @"${serverRoot}\ContactSourceRunner\ContactSources.properties");
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
            server.Set("DBServiceCentricityWfm.xml", @"${serverRoot}\ArchitectureServiceDescriptions\DBServiceCentricityWfm.xml");

            // Note how we configure multiple copies of the same file on the same app
            server.Set("Envision.jar", @"${serverRoot}\Envision.jar|${serverRoot}\WebServer\webapps\ET\WEB-INF\lib\Envision.jar|${serverRoot}\wwwroot\EnvisionComponents\Envision.jar");

            server.Set("envision_schema.xml", @"${serverRoot}\envision_schema.xml");
            server.Set("envision_schema_central.xml", @"${serverRoot}\envision_schema_central.xml");
            server.Set("EnvisionControls.cab", @"${serverRoot}\WebServer\webapps\ET\ETReporting\EnvisionControls.cab");
            server.Set("EnvisionServer.bat", @"${serverRoot}\EnvisionServer.bat");
            server.Set("EnvisionServer.exe_1", @"${serverRoot}\EnvisionServer.exe");
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

            // DatabaseUpdates
            server.Set("C2CUpdates.xml", @"${serverRoot}\DatabaseUpdates\C2CUpdates.xml");
            server.Set("CommonUpdates.xml", @"${serverRoot}\DatabaseUpdates\CommonUpdates.xml");
            server.Set("EWFMUpdates.xml", @"${serverRoot}\DatabaseUpdates\EWFMUpdates.xml");
            server.Set("manifest.xml_6", @"${serverRoot}\DatabaseUpdates\manifest.xml");
            server.Set("SpeechUpdates.xml", @"${serverRoot}\DatabaseUpdates\SpeechUpdates.xml");
            server.Set("MSSQLUpdate_build_10.0.1.1.xml", @"${serverRoot}\DatabaseUpdates\C2C\10.0\MSSQLUpdate_build_10.0.1.1.xml");
            server.Set("MSSQLUpdate_build_10.1.0.201.xml", @"${serverRoot}\DatabaseUpdates\C2C\10.1\MSSQLUpdate_build_10.1.0.201.xml");
            server.Set("MSSQLUpdate_build_10.1.0.65.xml", @"${serverRoot}\DatabaseUpdates\C2C\10.1\MSSQLUpdate_build_10.1.0.65.xml");
            server.Set("MSSQLUpdate_build_9.12.0.28.xml", @"${serverRoot}\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.28.xml");
            server.Set("MSSQLUpdate_build_9.12.0.37.xml", @"${serverRoot}\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.37.xml");
            server.Set("MSSQLUpdate_build_9.12.0.38.xml", @"${serverRoot}\DatabaseUpdates\Common\9.12\MSSQLUpdate_build_9.12.0.38.xml");
            server.Set("MSSQLUpdate_build_10.0.0.22.xml", @"${serverRoot}\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0.22.xml");
            server.Set("MSSQLUpdate_build_10.0.0.31.xml", @"${serverRoot}\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0.31.xml");
            server.Set("MSSQLUpdate_build_10.0.0303.1.xml", @"${serverRoot}\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.0303.1.xml");
            server.Set("MSSQLUpdate_build_10.0.1.1.xml_1", @"${serverRoot}\DatabaseUpdates\Common\10.0\MSSQLUpdate_build_10.0.1.1.xml");
            server.Set("MSSQLUpdate_build_10.1.0.140.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.140.xml");
            server.Set("MSSQLUpdate_build_10.1.0.151.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.151.xml");
            server.Set("MSSQLUpdate_build_10.1.0.172.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.172.xml");
            server.Set("MSSQLUpdate_build_10.1.0.236a.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.236a.xml");
            server.Set("MSSQLUpdate_build_10.1.0.236b.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.236b.xml");
            server.Set("MSSQLUpdate_build_10.1.0.242.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.242.xml");
            server.Set("MSSQLUpdate_build_10.1.0.333.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.333.xml");
            server.Set("MSSQLUpdate_build_10.1.0.47.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.47.xml");
            server.Set("MSSQLUpdate_build_10.1.0.61.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.61.xml");
            server.Set("MSSQLUpdate_build_10.1.0.62.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.62.xml");
            server.Set("MSSQLUpdate_build_10.1.0.65.xml_1", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.65.xml");
            server.Set("MSSQLUpdate_build_10.1.0.99.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.0.99.xml");
            server.Set("MSSQLUpdate_build_10.1.2.0.xml", @"${serverRoot}\DatabaseUpdates\Common\10.1\MSSQLUpdate_build_10.1.2.0.xml");

            server.Set("nativeServiceWin32.dll", @"${serverRoot}\nativeServiceWin32.dll|${serverRoot}\ContactSourceRunner\nativeServiceWin32.dll");
            server.Set("NetMerge.dll", @"${serverRoot}\ContactSourceRunner\NetMerge.dll");
            server.Set("NetMerge.pdb", @"${serverRoot}\ContactSourceRunner\NetMerge.pdb");
            server.Set("server.dll", @"${serverRoot}\bin\server.dll");
            server.Set("server.pdb", @"${serverRoot}\bin\server.pdb");
            server.Set("SIP_events.properties", @"${serverRoot}\ChannelManager\SIP_events.properties");
            server.Set("SourceRunnerService.exe", @"${serverRoot}\ContactSourceRunner\SourceRunnerService.exe");
            server.Set("SourceRunnerService.pdb", @"${serverRoot}\ContactSourceRunner\SourceRunnerService.pdb");
            server.Set("TeliaCallGuide.dll", @"${serverRoot}\ContactSourceRunner\TeliaCallGuide.dll");
            server.Set("TeliaCallGuide.pdb", @"${serverRoot}\ContactSourceRunner\TeliaCallGuide.pdb");
            server.Set("TokenService.xml", @"${serverRoot}\ArchitectureServiceDescriptions\TokenService.xml");
            server.Set("Tsapi.dll", @"${serverRoot}\ContactSourceRunner\Tsapi.dll");
            server.Set("Tsapi.pdb", @"${serverRoot}\ContactSourceRunner\Tsapi.pdb");
            server.Set("updater.jar", @"${serverRoot}\JRE\lib\ext\updater.jar");
            server.Set("WMWrapperService.xml", @"${serverRoot}\C2CServiceDescriptions\WMWrapperService.xml");

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

            // documentation
            server.Set("Centricity_Webhelp_DE", @"${serverRoot}\Help\_HelpSupervisorGerman\Centricity_Webhelp.zip");
            server.Set("Centricity_Webhelp_EN", @"${serverRoot}\Help\_HelpSupervisorEnglish\Centricity_Webhelp.zip");
            server.Set("Centricity_Webhelp_ES", @"${serverRoot}\Help\_HelpSupervisorSpanish\Centricity_Webhelp.zip");


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
            cm.Set("Set_ChannelDeviceIds.sql", @"${cmRoot}\SQLScripts\Set_ChannelDeviceIds.sql");
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


            IConfig ct = source.AddConfig("Centricity");
            ct.Set("ctRoot", @".");
            ct.Set("AgentInboxGrid.ascx", @"${ctRoot}\UserControls\Grids\AgentInboxGrid.ascx");
            ct.Set("Agents.aspx", @"${ctRoot}\Home\Agents\Agents.aspx");
            ct.Set("Agents.aspx.resx", @"${ctRoot}\Home\Agents\App_LocalResources\Agents.aspx.resx");
            ct.Set("Agents.aspx.de.resx", @"${ctRoot}\Home\Agents\App_LocalResources\Agents.aspx.de.resx");
            ct.Set("Agents.aspx.es.resx", @"${ctRoot}\Home\Agents\App_LocalResources\Agents.aspx.es.resx");
            ct.Set("App_Code.compiled", @"${ctRoot}\bin\App_Code.compiled");
            ct.Set("App_global.asax.compiled", @"${ctRoot}\bin\App_global.asax.compiled");
            ct.Set("App_GlobalResources.compiled", @"${ctRoot}\bin\App_GlobalResources.compiled");
            ct.Set("AttachedTrainingClipGrid.ascx", @"${ctRoot}\UserControls\Grids\AttachedTrainingClipGrid.ascx");
            ct.Set("centricity.dll", @"${ctRoot}\bin\centricity.dll");
            ct.Set("centricity.pdb", @"${ctRoot}\bin\centricity.pdb");
            ct.Set("Centricity_BLL.dll", @"${ctRoot}\bin\Centricity_BLL.dll");
            ct.Set("Centricity_BLL.pdb", @"${ctRoot}\bin\Centricity_BLL.pdb");
            ct.Set("Centricity_BLL.XmlSerializers.dll", @"${ctRoot}\bin\Centricity_BLL.XmlSerializers.dll");
            ct.Set("Centricity_DAL.dll", @"${ctRoot}\bin\Centricity_DAL.dll");
            ct.Set("Centricity_DAL.pdb", @"${ctRoot}\bin\Centricity_DAL.pdb");
            ct.Set("Centricity_deploy.resources.dll_DE", @"${ctRoot}\bin\de\Centricity_deploy.resources.dll");
            ct.Set("Centricity_deploy.resources.dll_DE_1", @"${ctRoot}\bin\de-DE\Centricity_deploy.resources.dll");
            ct.Set("Centricity_deploy.resources.dll_ES", @"${ctRoot}\bin\es\Centricity_deploy.resources.dll");
            ct.Set("Centricity_deploy.dll", @"${ctRoot}\bin\Centricity_deploy.dll");
            ct.Set("CentricityMaster.master.resx", @"${ctRoot}\App_LocalResources\CentricityMaster.master.resx");
            ct.Set("CentricityMaster.master.de.resx", @"${ctRoot}\App_LocalResources\CentricityMaster.master.de.resx");
            ct.Set("CentricityMaster.master.es.resx", @"${ctRoot}\App_LocalResources\CentricityMaster.master.es.resx");
            ct.Set("Centricity_SCA.dll", @"${ctRoot}\bin\Centricity_SCA.dll");
            ct.Set("Centricity_Shared.dll", @"${ctRoot}\bin\Centricity_Shared.dll");
            ct.Set("Centricity_Shared.pdb", @"${ctRoot}\bin\Centricity_Shared.pdb");
            ct.Set("Create_Centricity_WFM_SPROCS.sql", @"${ctRoot}\Create_Centricity_WFM_SPROCS.sql");
            ct.Set("Default.aspx", @"${ctRoot}\Home\Send\Default.aspx");
            ct.Set("EditEvaluation.aspx", @"${ctRoot}\PerformanceManagement\Evaluations\EditEvaluation.aspx");
            ct.Set("EnvisionTheme.css", @"${ctRoot}\App_Themes\EnvisionTheme\EnvisionTheme.css");
            ct.Set("EvaluationGrid.ascx", @"${ctRoot}\UserControls\Grids\EvaluationGrid.ascx");
            ct.Set("NewEvaluation.aspx", @"${ctRoot}\PerformanceManagement\Evaluations\NewEvaluation.aspx");
            ct.Set("RadEditor.skin", @"${ctRoot}\App_Themes\EnvisionTheme\RadEditor.skin");
            ct.Set("RAL.dll", @"${ctRoot}\bin\RAL.dll");
            ct.Set("RAL.pdb", @"${ctRoot}\bin\RAL.pdb");
            ct.Set("RecognitionDashboardItem.ascx", @"${ctRoot}\UserControls\DashboardControls\Recognition\RecognitionDashboardItem.ascx");
            ct.Set("Recognitions.aspx", @"${ctRoot}\PerformanceManagement\Recognitions\Recognitions.aspx");
            ct.Set("RecordingGridToolbar.ascx", @"${ctRoot}\UserControls\GridToolbar\RecordingGridToolbar.ascx");
            ct.Set("RecordingGridToolbar.ascx.resx", @"${ctRoot}\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.resx");
            ct.Set("RecordingGridToolbar.ascx.de.resx", @"${ctRoot}\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.de.resx");
            ct.Set("RecordingGridToolbar.ascx.es.resx", @"${ctRoot}\UserControls\GridToolbar\App_LocalResources\RecordingGridToolbar.ascx.es.resx");
            ct.Set("SiteToGroupAgentMover.ascx", @"${ctRoot}\UserControls\Movers\SiteToGroupAgentMover.ascx");
            ct.Set("SiteToGroupAgentMover.ascx.resx", @"${ctRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.resx");
            ct.Set("SiteToGroupAgentMover.ascx.de.resx", @"${ctRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.de.resx");
            ct.Set("SiteToGroupAgentMover.ascx.es.resx", @"${ctRoot}\UserControls\Movers\App_LocalResources\SiteToGroupAgentMover.ascx.es.resx");
            ct.Set("Telerik.Web.Design.dll", @"${ctRoot}\bin\Telerik.Web.Design.dll");
            ct.Set("Telerik.Web.UI.dll", @"${ctRoot}\bin\Telerik.Web.UI.dll");
            ct.Set("Telerik.Web.UI.xml", @"${ctRoot}\bin\Telerik.Web.UI.xml");
            ct.Set("TrainingClipGrid.ascx", @"${ctRoot}\UserControls\Grids\TrainingClipGrid.ascx");


            IConfig wmws = source.AddConfig("WMWrapperService");
            wmws.Set("wmwsRoot", @".");
            wmws.Set("DefaultEnvisionProfile.prx", @"${wmwsRoot}\DefaultEnvisionProfile.prx");
            wmws.Set("server.dll", @"${wmwsRoot}\server.dll");
            wmws.Set("WMWrapperService.exe", @"${wmwsRoot}\WMWrapperService.exe");


            IConfig dbmigration = source.AddConfig("DBMigration");
            dbmigration.Set("dbmigrationRoot", @".");
            dbmigration.Set("DBMigration_84SP9_To_10.sql", @"${dbmigrationRoot}\DBMigration_84SP9_To_10.sql");


            IConfig wfmsgusersync = source.AddConfig("WFMSGUserSync");
            wfmsgusersync.Set("wfmsgusersyncRoot", @".");
            wfmsgusersync.Set("WFMSGUserSync.exe", @"${wfmsgusersyncRoot}\WFMSGUserSync.exe");
            wfmsgusersync.Set("WFM_SyncUser.sql", @"${wfmsgusersyncRoot}\WFM_SyncUser.sql");


            IConfig avplayer = source.AddConfig("AVPlayer");
            avplayer.Set("avplayerRoot", @".");
            avplayer.Set("webapps_version", webapps_version);
            // shows up in patchFiles as "...\patchFiles\AVPlayer\AVPlayer"
            // where the first AVPlayer is the application name, and the
            // second AVPlayer is the subdir on disk
            avplayer.Set("AVPlayer.application", @"${avplayerRoot}\AVPlayer.application");
            avplayer.Set("AgentSupport.exe.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\AgentSupport.exe.deploy");
            avplayer.Set("AVPlayer.exe.config.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.config.deploy");
            avplayer.Set("AVPlayer.exe.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.deploy");
            avplayer.Set("AVPlayer.exe.manifest", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\AVPlayer.exe.manifest");
            avplayer.Set("CentricityApp.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\CentricityApp.dll.deploy");
            avplayer.Set("hasp_windows.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\hasp_windows.dll.deploy");
            avplayer.Set("Interop.WMPLib.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\Interop.WMPLib.dll.deploy");
            avplayer.Set("log4net.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\log4net.dll.deploy");
            avplayer.Set("nativeServiceWin32.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\nativeServiceWin32.dll.deploy");
            avplayer.Set("server.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\server.dll.deploy");
            avplayer.Set("SharedResources.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\SharedResources.dll.deploy");
            avplayer.Set("ISource.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\_ISource.dll.deploy");
            avplayer.Set("AVPlayer.resources.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\de\AVPlayer.resources.dll.deploy");
            avplayer.Set("AVPlayer.resources.dll.deploy_1", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\es\AVPlayer.resources.dll.deploy");
            avplayer.Set("CentricityApp.resources.dll.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            avplayer.Set("CentricityApp.resources.dll.deploy_1", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            avplayer.Set("AVPlayerIcon.ico.deploy", @"${avplayerRoot}\Application Files\AVPlayer_${webapps_version}\Resources\AVPlayerIcon.ico.deploy");


            IConfig rdtool = source.AddConfig("RecordingDownloadTool");
            rdtool.Set("rdtoolRoot", @".");
            rdtool.Set("webapps_version", webapps_version);
            rdtool.Set("RecordingDownloadTool.application", @"${rdtoolRoot}\RecordingDownloadTool.application");
            rdtool.Set("CentricityApp.dll.deploy_1", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\CentricityApp.dll.deploy");
            rdtool.Set("log4net.dll.deploy_1", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\log4net.dll.deploy");
            rdtool.Set("RecordingDownloadTool.exe.config.deploy", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.config.deploy");
            rdtool.Set("RecordingDownloadTool.exe.deploy", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.deploy");
            rdtool.Set("RecordingDownloadTool.exe.manifest", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\RecordingDownloadTool.exe.manifest");
            rdtool.Set("server.dll.deploy_1", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\server.dll.deploy");
            rdtool.Set("sox.exe.deploy", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\sox.exe.deploy");
            rdtool.Set("CentricityApp.resources.dll.deploy_2", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\de\CentricityApp.resources.dll.deploy");
            rdtool.Set("CentricityApp.resources.dll.deploy_3", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\es\CentricityApp.resources.dll.deploy");
            rdtool.Set("RecordingDownloadTool.resources.dll.deploy", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\de\RecordingDownloadTool.resources.dll.deploy");
            rdtool.Set("RecordingDownloadTool.resources.dll.deploy_1", @"${rdtoolRoot}\Application Files\RecordingDownloadTool_${webapps_version}\es\RecordingDownloadTool.resources.dll.deploy");

            source.ExpandKeyValues();
            source.Save("Aristotle_targets.config");
        }

        // Each application passes in a list of keys that identifies a directory to patch.  Copy all files from the
        // source directory into the patch cache.  The directory's name is configurable and may be different.
        public void makePortablePatch(string appToPatch, string buildRoot)
        {
            // applicationCache is where the files are staged for creating the patch.  buildRoot is the original
            // location from which the files are copied.  It takes the place of the Nini config.  The buildRoot
            // includes directory name (e.g., RadControls).
            string applicationCache = Path.Combine(this.SourceDir, appToPatch);

            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(buildRoot, applicationCache);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                ExitEarly.exit("caught System.IO.DirectoryNotFoundException");
            }
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
                replaced = Archiver.dotToDash(regex.Match(toFormat));
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
                    ExitEarly.exit(SourceDir + " is not a valid directory");
                }

                // these files install and log the patch
                zip.AddFile("Clyde.exe");
                zip.AddFile("PatchLib.dll");
                zip.AddFile("Nini.dll");
                zip.AddFile("NLog.dll");
                zip.AddFile("NLog.config");
                // for zipping up the backed up install folders (so now we're
                // creating a zip file that extracts itself, does some stuff
                // then creates another zip file)
                zip.AddFile("Ionic.Zip.dll");

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
            Console.SetWindowSize(120, 50);
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

        public void Backup(List<Installer> installs, List<string> skipList)
        {
            string zipFile = Path.Combine(this.BackupDir, BackupZipFileName());

            using (ZipFile zip = new ZipFile())
            {
                foreach (Installer i in installs)
                {
                    string backupTo = Path.Combine(this.BackupDir, i.displayName);
                    logger.Info("Backing up install folder {0} to {1}", i.installLocation, backupTo);
                    CopyFolder(i.installLocation, backupTo, skipList);
                }
                zip.AddDirectory(this.BackupDir);
                logger.Info("Saving zip file to {0}", zipFile);
                zip.Save(zipFile);
            }
        }

        private string BackupZipFileName()
        {
            string dateTime = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            return "pre-" + this.PatchVersion + "-" + dateTime + ".zip";
        }

        public void Patch(List<ETApplication> apps)
        {
            foreach (ETApplication app in apps)
            {
                logger.Info("Patching an application in {0}", app.displayName);

                if (app.replaceAll == true)
                {
                    Directory.Delete(app.patchTo, true);
                    Directory.CreateDirectory(app.patchTo);
                    CopyFolder(app.patchFrom, app.patchTo, null, true);
                }
                else
                {
                    CopyFolder(app.patchFrom, app.patchTo, null, true);
                }
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

        public static void CopyFolder(string sourceFolder, string destFolder, List<string> skipList = null, bool patching = false)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder);
            bool skipThis = false;
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);

                if (skipList != null)
                {
                    foreach (string s in skipList)
                    {
                        if (name.StartsWith(s, true, null))
                        {
                            logger.Info("NOT backing up {0} (full path: {1}); startswith:{2}", name, file, s);
                            skipThis = true;
                            break;
                        }
                        else
                        {
                            logger.Info("    backing up {0} (full path: {1})", name, file);
                        }
                    }
                }
                // I'm not a big fan of this indirection, but it should get the job done.  There are other ways to
                // break out of an inner loop with LINQ, but not in .NET 2.0.
                if (skipThis == true)
                {
                    break;
                }

                try
                {
                    if (patching == true)
                    {
                        logger.Info("patching {0}; source: {1}, destination: {2}", name, file, dest);
                    }
                    File.Copy(file, dest, true);
                }
                catch (FileNotFoundException)
                {
                    logger.Warn("a file to replace was not found: {0}", file);
                }
                catch (IOException ex)
                {
                    ExitEarly.exit(ex.ToString());
                }
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest, skipList, patching);
            }
        }

        public void CheckETServices()
        {
            List<string> ETServices = new List<string>();
            ETServices.Add("ChanMgrSvc");
            ETServices.Add("EnvisionServer");
            ETServices.Add("ETService");
            ETServices.Add("SourceRunnerService");
            // this is not a service, but a child process of SourceRunnerService
            ETServices.Add("ETContactSource.exe");
            ETServices.Add("tomcat6");
            ETServices.Add("WMWrapperService");

            foreach (string p in ETServices)
            {
                if (IsServiceRunning(p))
                {
                    ExitEarly.exit("An Envision service (" + p + ") is running.  Exiting.");
                }
            }
        }

        public static bool IsServiceRunning(string serviceName)
        {
            bool isRunning = false;

            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.StartsWith(serviceName))
                {
                    isRunning = true;
                }
            }
            return isRunning;
        }
    }
}
