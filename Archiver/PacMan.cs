using System;
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
