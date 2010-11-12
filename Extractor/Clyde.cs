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
            Console.WriteLine("e.AppDir: {0}", e.AppDir);
            Console.WriteLine("e.ExtractDir: {0}", e.ExtractDir);

            // beware System.IO.DirectoryNotFoundException
            e.rollCall(Path.Combine(e.ExtractDir, "ROOT"), e.AppDir);

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
        }
    }
}
