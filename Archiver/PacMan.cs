using Microsoft.Test.CommandLineParsing;
using System;
using System.IO;

// using command-line parser from TestAPI
// http://testapi.codeplex.com/

namespace PatchTool
{
    public class PacMan
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                string usage = "PacMan.exe\n\n";
                usage += "Required argument:\n";
                usage += "\t-archive=<src-dir>";
                usage += "\n\n";
                usage += "Optional arguments:\n";
                usage += "\t-patchID=<patchID>\n";
                usage += "\t-productVersion=<productVersion>\n";
                //usage += "\t-extractDir=<extractDir>\n";
                // TC: this doesn't do anything yet
                usage += "\t-?\n";

                System.Windows.Forms.MessageBox.Show(usage, "PacMan needs more info");
                return;
            }

            CommandLineDictionary d = CommandLineDictionary.FromArguments(args, '-', '=');
            Archiver a = new Archiver();
            string src_dir;
            string patch_id;
            //string extract_dir;
            string product_version;

            if (d.ContainsKey("archive"))
            {
                d.TryGetValue("archive", out src_dir);
                a.SourceDir = src_dir;
            }
            else
            {
                // "pretty it up" and exit
                throw new ArgumentException("something's broken!");
            }

            if (d.ContainsKey("patchID"))
            {
                //Console.WriteLine("setting patchID");
                d.TryGetValue("patchID", out patch_id);
                a.PatchID = patch_id;
            }
            else
            {
                Console.WriteLine("Warning: using default patch ID: {0}", a.PatchID);
            }
            //if (d.ContainsKey("extractDir"))
            //{
            //    //Console.WriteLine("setting extractDir");
            //    d.TryGetValue("extractDir", out extract_dir);
            //    a.ExtractDir = extract_dir;
            //}
            if (d.ContainsKey("productVersion"))
            {
                //Console.WriteLine("setting productVersion");
                d.TryGetValue("productVersion", out product_version);
                a.ProductVersion = product_version;
            }
            else
            {
                Console.WriteLine("Warning: using default product version: {0}", a.ProductVersion);
            }

            a.ExtractDir = Path.Combine(a.SourceDir, "patches", a.ProductVersion, "tmp");
            a.run();
        }
    }
}
