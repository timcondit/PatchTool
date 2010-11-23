using Microsoft.Test.CommandLineParsing;    // http://testapi.codeplex.com/
using System;
using System.IO;
using System.Windows.Forms;

namespace PatchTool
{
    public class PacMan
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                string usage = "PacMan.exe\n\n";
                usage += "Required:\n";
                usage += "\t-appName\tthe name of the target app, e.g., ServerSuite\n";
                usage += "\t-sourceDir\t\tthe path to the patch contents\n";
                usage += "\t-patchVersion\tthe version number for this patch\n";
                usage += "Optional:\n";
                usage += "\t-?\t\tthis doesn't do anything yet";
                MessageBox.Show(usage, "PacMan needs more info");
                return;
            }

            CommandLineDictionary d = CommandLineDictionary.FromArguments(args, '-', '=');
            Archiver a = new Archiver();

            // application identifier, e.g., ServerSuite, ChannelManager, etc.  Maybe this should
            // be an enumeration
            string app_name;
            // where's the patch contents?
            string src_dir;
            string patch_version;

            try
            {
                d.TryGetValue("sourceDir", out src_dir);
                a.SourceDir = src_dir;

                d.TryGetValue("appName", out app_name);
                a.AppName = app_name;

                d.TryGetValue("patchVersion", out patch_version);
                a.PatchVersion = patch_version;
            }
            catch (System.ArgumentNullException e)
            {
                Console.WriteLine("Something broke while parsing command-line arguments");
                Console.WriteLine();
                Console.WriteLine(e.StackTrace);
                throw;
            }

            //  If the files are stored in C:\patches\<APPNAME>\<PATCHVER>, and that location
            // already exists, error and exit.
            //
            a.ExtractDir = Path.Combine(@"C:\patches", a.AppName, a.PatchVersion);
            a.run();
        }
    }
}
