using Ionic.Zip;
using Microsoft.Test.CommandLineParsing;
using System;
using System.IO;
using System.Collections.Generic;

// using DotNetZip library
// http://dotnetzip.codeplex.com/
// http://cheeso.members.winisp.net/DotNetZipHelp/html/d4648875-d41a-783b-d5f4-638df39ee413.htm
//
// using TestAPI
// http://testapi.codeplex.com/

// TODO
// 1) DotNetZip: look at ExtractExistingFileAction OverwriteSilently
//  http://cheeso.members.winisp.net/DotNetZipHelp/html/5443c4c0-6f74-9ae1-37fd-9a4ae936832d.htm
// 2) The Archiver and Extractor classes should be combined into a DLL in a new namespace, maybe
//  called Updater or PatchTool or something.  Then PacMan and Clyde can be entry points that use
//  the Updater (or PatchTool) to do their bidness.


namespace PatchTool
{
    public class Archiver
    {
        private string _sourceDir;
        public string SourceDir
        {
            get { return _sourceDir; }
            set { _sourceDir = value; }
        }

        private string _patchId = "GenericArchive.exe";
        public string PatchID
        {
            get { return _patchId; }
            set { _patchId = value; }
        }

        private string _productVersion = "1.0.0.0";
        public string ProductVersion
        {
            get { return _productVersion; }
            set { _productVersion = value; }
        }

        // TC: this should probably be in the Extractor only
        private string _extractDir = @".";
        public string ExtractDir
        {
            get { return _extractDir; }
            set { _extractDir = value; }
        }

        public void run()
        {
            using (ZipFile zip = new ZipFile())
            {
                if (Directory.Exists(SourceDir))
                {
                    zip.AddDirectory(SourceDir, Path.GetFileName(SourceDir));
                }
                else
                {
                    Console.WriteLine("{0} is not a valid directory", SourceDir);
                    // NOT A BIG FAN of these types of side-effects
                    System.Environment.Exit(1);
                }

                zip.Comment = "Where will this show up?";

                SelfExtractorSaveOptions options = new SelfExtractorSaveOptions();
                options.Flavor = SelfExtractorFlavor.ConsoleApplication;
                options.ProductVersion = ProductVersion;
                options.DefaultExtractDirectory = ExtractDir;

                // TC: I don't need these yet, but it's so cool that they're available.
                //options.PostExtractCommandLine = "ExeToRunAfterExtract";
                //options.RemoveUnpackedFilesAfterExecute = true;

                // TC: delete PatchID before reusing!
                zip.SaveSelfExtractor(PatchID, options);
            }

            //string DirectoryPath = ".";
            //using (ZipFile zip = new ZipFile())
            //{
            //    zip.AddDirectory(DirectoryPath, System.IO.Path.GetFileName(DirectoryPath));
            //    zip.Comment = "This will be embedded into a self-extracting console-based exe";
            //    SelfExtractorSaveOptions options = new SelfExtractorSaveOptions();
            //    options.Flavor = SelfExtractorFlavor.ConsoleApplication;
            //    options.DefaultExtractDirectory = "%USERPROFILE%\\ExtractHere";
            //    options.PostExtractCommandLine = "ExeToRunAfterExtract";
            //    options.RemoveUnpackedFilesAfterExecute = true;
            //    zip.SaveSelfExtractor("archive.exe", options);
            //}
        }

        // TC: given name of ExistingZipFile (param), list the file's contents.  This
        //  method will be mostly useful, if a little self-referential (it could be called
        //  on itself! :)), in Extractor.
        //public void List()
        //{
        //    using (ZipFile zip = ZipFile.Read(ExistingZipFile))
        //    {
        //        foreach (ZipEntry e in zip)
        //        {
        //            if (header)
        //            {
        //                System.Console.WriteLine("Zipfile: {0}", zip.Name);
        //                if ((zip.Comment != null) && (zip.Comment != ""))
        //                    System.Console.WriteLine("Comment: {0}", zip.Comment);
        //                System.Console.WriteLine("\n{1,-22} {2,8}  {3,5}   {4,8}  {5,3} {0}",
        //                                         "Filename", "Modified", "Size", "Ratio", "Packed", "pw?");
        //                System.Console.WriteLine(new System.String('-', 72));
        //                header = false;
        //            }
        //            System.Console.WriteLine("{1,-22} {2,8} {3,5:F0}%   {4,8}  {5,3} {0}",
        //                                     e.FileName,
        //                                     e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
        //                                     e.UncompressedSize,
        //                                     e.CompressionRatio,
        //                                     e.CompressedSize,
        //                                     (e.UsesEncryption) ? "Y" : "N");
        //        }
        //    }
        //}
    }

    public class Extractor
    {
        public Extractor() { }
        public Extractor(string _appDir)
        {
            AppDir = _appDir;
        }

        private string _appDir;
        public string AppDir
        {
            get { return _appDir; }
            set { _appDir = value; }
        }

        public void run()
        {
            return;
        }
    }
}
