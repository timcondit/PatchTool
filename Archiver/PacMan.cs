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
            [Option("s", "sourceDir",
                    Required = true,
                    HelpText = "The path to the patch's contents.")]
            public string sourceDir = String.Empty;

            [Option("r", "patchVersion",
                    Required = true,
                    HelpText = "The version number for this patch.")]
            public string patchVersion = String.Empty;

            //[Option("v", null,
            //        HelpText = "Verbose level. Range: from 0 to 2.")]
            //public int? VerboseLevel = null;

            //[Option("i", null,
            //       HelpText = "If file has errors don't stop processing.")]
            //public bool IgnoreErrors = false;

            //[Option("j", "jump",
            //        HelpText = "Data processing start offset.")]
            //public double StartOffset = 0;

            [HelpOption(
                    HelpText = "Display this help screen.")]

            public string GetUsage()
            {
                var help = new HelpText("Envision Package Manager");
                help.AdditionalNewLineAfterOption = true;
                help.Copyright = new CopyrightInfo("Envision Telephony, Inc.", 2011);
                help.AddPreOptionsLine("Usage: PacMan -s<sourceDir> -r<patchVersion>");
                help.AddPreOptionsLine("       PacMan -?");
                help.AddOptions(this);

                return help;
            }
            #endregion
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // TC: this will do for now until I can use the new help text
            if (args.Length < 2)
            {
                string usage = "PacMan.exe\n\n";
                usage += "Required:\n";
                usage += "\t--sourceDir\tthe path to the patch contents\n";
                usage += "\t--patchVersion\tthe version number for this patch\n";
                MessageBox.Show(usage, "PacMan needs more info");

                logger.Info("Not enough arguments provided.  Show usage and exit.");
                return;
            }

            Archiver a = new Archiver();
            a.makeSourceConfig();
            a.makeTargetConfig();

            // Should have the patchVersion before calling a.makePortablePatch().  Actually, should really break the
            // configuration out into a separate utility.

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            // Comment out if not needed for the current patch.
            IEnumerable<string> serverKeys = new List<string> { "Envision.jar", "envision_schema.xml",
                "envision_schema_central.xml", "ETScheduleService.xml", "ChannelBrokerService.xml", "CiscoICM.dll",
                "cstaLoader.dll", "cstaLoader_1_2.dll", "cstaLoader_1_3_3.dll", "cstaLoader_3_33.dll",
                "cstaLoader_9_1.dll", "cstaLoader_9_5.dll", "ctcapi32.dll", "ctcLoader_6.0.dll", "ctcLoader_7.0.dll",
                "NetMerge.dll", "SourceRunnerService.exe", "TeliaCallGuide.dll", "Tsapi.dll", "CommonUpdates.xml",
                "MSSQLUpdate_build_10.0.0303.1.xml"
            };
            a.makePortablePatch("Server", serverKeys);

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            // Comment out if not needed for the current patch.
            IEnumerable<string> cmKeys = new List<string> { "audiocodesChannel.dll", "audiocodesChannel.pdb",
                "AvayaVoipChannel.dll", "AvayaVoipChannel.pdb", "ChanMgrSvc.exe", "ChanMgrSvc.pdb",
                "DemoModeChannel.dll", "DemoModeChannel.pdb", "DialogicChannel.dll", "DialogicChannel.pdb",
                "DialogicChannel60.dll", "DialogicChannel60.pdb", "DMCCConfigLib.dll", "DMCCConfigLib.pdb",
                "DMCCWrapperLib.dll", "DMCCWrapperLib.pdb", "DMCCWrapperLib.tlb", "IPXChannel.dll", "IPXChannel.pdb",
                "RtpTransmitter.dll", "RtpTransmitter.pdb", "EnvisionSR.bat", "EnvisionSR.reg", "instsrv.exe",
                "sleep.exe", "srvany.exe", "svcmgr.exe"
            };
            a.makePortablePatch("ChannelManager", cmKeys);

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            // Comment out if not needed for the current patch.
            IEnumerable<string> wmwsKeys = new List<string> {
                "DefaultEnvisionProfile.prx"
            };
            a.makePortablePatch("WMWrapperService", wmwsKeys);

            // This is where we specify which files go into the patch (Server). It will be manually updated for now.
            // Comment out if not needed for the current patch.
            //IEnumerable<string> toolsKeys = new List<string> { "" };
            //a.makePortablePatch("Tools", toolsKeys);


            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            // where's the patch contents?
            if (options.sourceDir == String.Empty)
            {
                // "pretty it up" and exit
                throw new ArgumentException("something's broken! (options.sourceDir)");
            }
            else
            {
                a.SourceDir = options.sourceDir;
            }

            if (options.patchVersion == String.Empty)
            {
                // "pretty it up" and exit
                throw new ArgumentException("something's broken! (options.patchVersion)");
            }
            else
            {
                a.PatchVersion = options.patchVersion;
            }

            // TC: If the files are stored in C:\patch_staging\<APPNAME>\<PATCHVER>, and that
            // location already exists, error and exit.
            //
            // The extract dir is set before the archive is created.  There is NOTHING that can be
            // done (as far as I know) at extraction time to change that.  Bottom line is, the
            // extractDir cannot be APPDIR.  Which sucks, but oh well.
            string extractDirTmp = Path.Combine(@"C:\patch_staging", a.PatchVersion);
            a.ExtractDir = Path.Combine(extractDirTmp, a.PatchVersion);
            a.run();
        }
    }
}
