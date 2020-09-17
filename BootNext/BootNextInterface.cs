using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace BootNext
{
    public static class BootNextInterface
    {
        public static void BootTo(string path, string key)
        {
            var targetPath = Path.Combine(path, "next_boot");
            File.WriteAllText(targetPath, key);
        }

        public static void InstallTo(Partition efi)
        {
            var path = efi.GetDriveLetter();
            if (path == null)
            {
                efi.AssignDriveLetter();
                path = efi.GetDriveLetter()!;
            }

            var targetPath = DiskScanner.GetBootNextDir(path);
            Directory.CreateDirectory(targetPath);
            
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            foreach (var file in Directory.GetFiles(Path.Combine(dir, "Resources", "BootNext")))
            {
                if (file == null) continue;

                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetPath, fileName), true);
            }
        }
    }
}