using Ionic.Zip;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

// using DotNetZip library
// http://dotnetzip.codeplex.com/
// http://cheeso.members.winisp.net/DotNetZipHelp/html/d4648875-d41a-783b-d5f4-638df39ee413.htm
//
// TODO
// 1: look at ExtractExistingFileAction OverwriteSilently
//  http://cheeso.members.winisp.net/DotNetZipHelp/html/5443c4c0-6f74-9ae1-37fd-9a4ae936832d.htm
// 2: 

// using RelativePath method from http://mrpmorris.blogspot.com/2007/05/convert-absolute-path-to-relative-path.html
// Don't forget to use System.Text;

// NOTES
// 1: Extractor will have to maintain the list of registry keys for each application that is
//    patched.  But Archiver will need to pass a hint to let Extractor know which registry key to
//    query.

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
                //Console.WriteLine("options.DefaultExtractDirectory: {0}", options.DefaultExtractDirectory);

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

    // The Extractor needs a few paths:
    //  - ExtractDir is where the files are extracted when the user double clicks the SFX.  This path should be logged.
    //  - APPDIR/patches/<version>/old is where the files to replace are stored for posterity.  This info should be logged.
    //  - APPDIR/patches/<version>/new is where the new files are stored for posterity.  This info should be logged.
    //  - APPDIR is where the patch files are eventually delivered to, if all goes well.

    public class Extractor
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("patch.log");
        string logmsg;

        private void init()
        {
            Console.SetWindowSize(120, 50);
        }

        public Extractor()
        {
            init();
        }

        public Extractor(string _appDir)
        {
            init();
            AppDir = _appDir;
            //logmsg = System.String.Format("APPDIR: {0}", AppDir);
            //log.Info(logmsg);
            log.Info(System.String.Format("APPDIR: {0}", AppDir));
        }

        private string _appDir;
        public string AppDir
        {
            get { return _appDir; }
            set { _appDir = value; }
        }

        // This should be equivalent to ExtractDir in Archiver.  I should probably find a better
        // solution.
        private string _extractDir = Directory.GetCurrentDirectory();
        public string ExtractDir
        {
            get { return _extractDir; }
        }

        // 0: Get APPDIR, or not.  Maybe create a method that accepts APPDIR, and start with 1:
        // 1: Assemble path dictionaries.  Starting with relative paths in ROOT, verify every file
        //    in the same relative location under APPDIR.  Do NOT touch anything yet.
        // 2: Create APPDIR/patches/<version>/{old|new}
        // 3: ?? Create a mirror directory structure in old/ & new/?
        // 4: Start copying files to old/.
        // 5: Next?

        // Are all files in srcDir also present in dstDir (extractDir and appDir, respectively)?
        //
        // TC: void or bool?
        //
        // NB: may need "C:\patches\d7699dbd-8214-458e-adb0-8317dfbfaab1>runas /env /user:administrator Clyde.exe"
        public bool rollCall(string _srcDir, string _dstDir)
        {
            DirectoryInfo srcDir = new DirectoryInfo(_srcDir);
            DirectoryInfo dstDir = new DirectoryInfo(_dstDir);
            FileInfo[] srcFiles = srcDir.GetFiles("*", SearchOption.AllDirectories);

            // the file name stripped from the full path; base paths are the heads
            string tail;
            // the files of the patch and installation
            string srcFile;
            string dstFile;
            // the files of the new and old directories
            string newFile;
            string oldFile;

            // create backup folders: Clyde needs the patchID; fake it for now
            string[] newParts = { srcDir.ToString(), "patches", @"1.2.3.4" };
            DirectoryInfo newDir = new DirectoryInfo(Path.Combine(newParts));
            if (!Directory.Exists(newDir.ToString()))
            {
                try
                {
                    Directory.CreateDirectory(newDir.ToString());
                }
                catch (System.UnauthorizedAccessException)
                {
                    MessageBox.Show("PatchTool must be run as Administrator on this system", "sorry Charlie");
                    throw;
                }
            }
            //
            string[] oldParts = { dstDir.ToString(), "patches", @"1.2.3.4" };
            DirectoryInfo oldDir = new DirectoryInfo(Path.Combine(oldParts));
            if (!Directory.Exists(oldDir.ToString()))
            {
                try
                {
                Directory.CreateDirectory(oldDir.ToString());
                }
                catch (System.UnauthorizedAccessException)
                {
                    MessageBox.Show("PatchTool must be run as Administrator on this system", "sorry Charlie");
                    throw;
                }
            }
            
            // flag to decide whether to patch system
            //bool soFarSoGood = true;

            foreach (FileInfo f in srcFiles)
            {
                // TC: add a Console title (in Clyde, not here)
                // TC: tell the user what we're doing (in Clyde, not here)
                tail = RelativePath(srcDir.ToString(), f.FullName);
                // get and check original locations
                srcFile = Path.GetFullPath(Path.Combine(srcDir.ToString(), tail));
                dstFile = Path.GetFullPath(Path.Combine(dstDir.ToString(), tail));
                statFile(srcFile);
                statFile(dstFile);

                // backup locations
                newFile = Path.GetFullPath(Path.Combine(newDir.ToString(), tail));
                oldFile = Path.GetFullPath(Path.Combine(oldDir.ToString(), tail));
                statFile(newFile);
                statFile(oldFile);

                // TC: TODO check state before changing anything, and log any discrepancies.  That
                // will probably mean I loop over the files of the patch in two passes, so this
                // stuff will be moved out (above).
                //if (!(statFile(srcFile) && statFile(dstFile)))
                //{
                //    soFarSoGood = false;

                //    // copy newFile to APPDIR/patches/<version>/new/
                //    // copy oldFile to APPDIR/patches/<version>/old/
                //}
                //statFile(srcFile);
                //statFile(dstFile);
                //statFile(newFile);
                //statFile(oldFile);
                Console.WriteLine();

                // if directory exists:
                //   if file not exists:
                //     copy
                //   else:
                //     exit
                // NO.  This should already be done. 
            }

            // return soFarSoGood?
            return true;
        }

        private bool statFile(string f)
        {
            FileInfo ff = new FileInfo(f);
            if (ff.Exists)
            {
                Console.Write("{0, -110}", ff, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.Write("found:\t\t{0}", ff, Console.WindowWidth, Console.WindowHeight);
                Console.WriteLine(String.Format("{0, 9}", "[present]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.Write("{0, -110}", ff, Console.WindowWidth, Console.WindowHeight);
                Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.Write("found:\t\t{0}", ff, Console.WindowWidth, Console.WindowHeight);
                Console.WriteLine(String.Format("{0, 9}", "[missing]"), Console.WindowWidth, Console.WindowHeight);
                Console.ResetColor();
                return true;
            }
        }

        public void run()
        {
            return;
        }

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
    }
}
