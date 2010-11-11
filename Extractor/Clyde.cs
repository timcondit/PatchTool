using Microsoft.Win32;
using System;

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
            Console.WriteLine("e.appDir: {0}", e.AppDir);

            // TC: it's time to ...
        }
    }
}
