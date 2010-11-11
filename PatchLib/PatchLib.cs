using Ionic.Zip;
using System;
using System.IO;
using System.Collections.Generic;

// using DotNetZip library
// http://dotnetzip.codeplex.com/
// http://cheeso.members.winisp.net/DotNetZipHelp/html/d4648875-d41a-783b-d5f4-638df39ee413.htm
//
// TODO
// 1) look at ExtractExistingFileAction OverwriteSilently
//  http://cheeso.members.winisp.net/DotNetZipHelp/html/5443c4c0-6f74-9ae1-37fd-9a4ae936832d.htm
// 2) 


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

        private string _extractDir = ".";
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

                // TC: well this is ugly.  I've added a reference to Extractor.exe into the
                // Archiver project, but it seems like this belongs in Archiver as well, since
                // that's what's looking for the file.
                zip.AddFile("Clyde.exe");
                zip.AddFile("PatchLib.dll");

                zip.Comment = "Where will this show up?";

                SelfExtractorSaveOptions options = new SelfExtractorSaveOptions();
                options.Flavor = SelfExtractorFlavor.ConsoleApplication;
                // TC: I'd like to also use options.FileVersion here.  Maybe sort it out later.
                options.ProductVersion = ProductVersion;
                options.DefaultExtractDirectory = ExtractDir;
                // TC: debug
                Console.WriteLine("options.DefaultExtractDirectory: {0}", options.DefaultExtractDirectory);

                options.Copyright = "Copyright 2010 Envision Telephony";
                options.PostExtractCommandLine = "Clyde.exe";
                // TC: false for dev, true (maybe) for production
                options.RemoveUnpackedFilesAfterExecute = false;

                // TC: delete other patches before reusing file name!
                zip.SaveSelfExtractor(PatchID, options);
            }
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
