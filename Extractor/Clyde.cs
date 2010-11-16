using Microsoft.Win32;
using System;
using System.IO;

namespace PatchTool
{
    class Clyde
    {
        static void Main(string[] args)
        {
            // TC: Clyde needs no arguments.  The APPDIR is read from the registry; the
            //  productVersion is set when the patch is created; and the extractDir is composed
            //  from all three.

            // TC: read the APPDIR from the registry
            RegistryKey hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"SOFTWARE\Envision\Click2Coach\Server");
            Extractor e = new Extractor();
            e.AppDir = hklm.GetValue("InstallPath", "rootin tootin").ToString();
            // TC: debug
            //Console.WriteLine("e.AppDir: {0}", e.AppDir);
            //Console.WriteLine("e.ExtractDir: {0}", e.ExtractDir);

            // beware System.IO.DirectoryNotFoundException
            //
            // NB: may need "C:\patches\d7699dbd-8214-458e-adb0-8317dfbfaab1>runas /env /user:administrator Clyde.exe"
            try
            {
                // TC: add a Console title (somewhere, maybe not here)
                // TC: tell the user what we're doing here (pre-file-move check)
                // TC: add simple continue or cancel here?
                e.run(Path.Combine(e.ExtractDir, "ROOT"), e.AppDir);
            }
            catch (System.UnauthorizedAccessException)
            {
                System.Environment.Exit(1);
            }

            // TC: next steps
            // 0. Got folder with app name in APPDIR. Example: patches/10.1.0001.0/ServerSuite.
            //    Also Clyde.exe and PatchLib.dll in 10.1.0001.0.  [verify]
            //
            // 1. cp ServerSuite/ to ../new. If ../new already exists exit immediately
            //
            // 2. For each file and folder in ServerSuite/, copy from the original location to
            //    patches/10.1.0001.0/old. This could be some work. How to stub out for today?
            //
            // 3. Repeat no. 2, but this time overwrite files in APPDIR with files in new/. If file
            //    not overwritten roll back and exit or warn?
            //
            // 4. Find a way to streamline nos. 2&3
            //
            // 5. ... ?

            //FileInfo f = new FileInfo(@"C:\source\git\PatchTool\Archiver\obj\x86\Release\Archiver.csproj.FileListAbsolute.txt");
            //f.CopyTo(@"C:\create\this\dir");

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
