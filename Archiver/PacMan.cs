using System;
using Microsoft.Test.CommandLineParsing;

namespace PatchTool
{
    public class PacMan
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                //Console.WriteLine("Usage: PacMan [-archive=<src-dir> | -extract=<app-dir>]");
                Console.WriteLine("Required:\tPacMan -archive=<src-dir>");
                Console.WriteLine();
                Console.WriteLine("Optional:\t[-patchID=<patchID>]");
                Console.WriteLine("\t\t[-productVersion=<productVersion>]");
                Console.WriteLine("\t\t[-extractDir=<extractDir>]");
                Console.WriteLine("\t\t[-?]");
                return;
            }

            CommandLineDictionary d = CommandLineDictionary.FromArguments(args, '-', '=');
            Archiver a = new Archiver();
            string src_dir;
            string patch_id;
            string extract_dir;
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
            if (d.ContainsKey("extractDir"))
            {
                //Console.WriteLine("setting extractDir");
                d.TryGetValue("extractDir", out extract_dir);
                a.ExtractDir = extract_dir;
            }
            if (d.ContainsKey("productVersion"))
            {
                //Console.WriteLine("setting productVersion");
                d.TryGetValue("productVersion", out product_version);
                a.ProductVersion = product_version;
            }
            a.run();
        }
    }
}
