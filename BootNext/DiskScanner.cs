using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace BootNext
{
    public static class DiskScanner
    {
        public static IEnumerable<Partition> FindEfis()
        {
            var scope = new ManagementScope(@"\\.\ROOT\Microsoft\Windows\Storage");
            var query = new ObjectQuery($"SELECT * FROM MSFT_Partition WHERE GptType = '{{{Partition.EspGuid.ToString()}}}'");
            var searcher = new ManagementObjectSearcher(scope, query);

            return searcher.Get()
                .OfType<ManagementObject>()
                .Select(o => new Partition(o));
        }

        public static bool HasBootNextInstalled(Partition part)
        {
            if (part.IsOffline)
            {
                try
                {
                    part.Online();
                }
                catch (DiskOperationException e)
                {
                    Debug.WriteLine($"Could not take partition online!");
                    Debug.WriteLine(e);
                    return false;
                }
            }

            var path = GetBootNextDir(part);
            return Directory.Exists(path);
        }
        
        public static IEnumerable<Partition> FindInstalledEfis()
        {
            var parts = FindEfis();
            return parts.Where(HasBootNextInstalled);
        }

        public static string GetBootNextDir(string partPath)
        {
            return Path.Combine(partPath, "EFI", "BootNext");
        }
        public static string? GetBootNextDir(Partition part)
        {
            var paths = part.GetAccessPaths();
            return paths.Length == 0 ? null : GetBootNextDir(paths[0]);
        }
    }
}