using Microsoft.Win32;
using System;
using System.IO;

namespace PatchTool
{
    class Clyde
    {
        static void Main(string[] args)
        {
            // TC: read the APPDIR from the registry
            RegistryKey hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"SOFTWARE\Envision\Click2Coach\Server");

            // TC: Clyde needs no arguments.  APPDIR is read from the registry; patchVersion is set
            // when the patch is created; and extractDir is composed from all three.
            //
            // Late note: this is about to be not true.  Clyde needs the patch name (in the form of
            // APPNAME-PATCHVER.exe).
            Extractor e = new Extractor();
            e.AppDir = hklm.GetValue("InstallPath", "rootin tootin").ToString();

            // beware System.IO.DirectoryNotFoundException
            //
            // NB: may need "C:\patches\d7699dbd-8214-458e-adb0-8317dfbfaab1>runas /env /user:administrator Clyde.exe"
            try
            {
                // TC: few things TODO
                // 1: add a Console title (somewhere, maybe not here)
                // 2:: tell the user what we're doing here (pre-file-move check)
                // 3: add simple continue or cancel here?
                // 4: get rid of "ROOT" - should be "e.run(e.ExtractDir, e.AppDir);"
                e.run(Path.Combine(e.ExtractDir, "ROOT"), e.AppDir);
            }
            catch (System.UnauthorizedAccessException)
            {
                System.Environment.Exit(1);
            }

            // TC: for testing
            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
    }
}
