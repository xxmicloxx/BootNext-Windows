using System;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace BootNext
{
    [Serializable]
    public class DiskOperationException : Exception
    {
        public DiskOperationException(string msg) : base($"Could not execute disk operation: {msg}")
        {
        }

        public DiskOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DiskOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    
    public class Partition
    {
        public static readonly Guid EspGuid = Guid.Parse("{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}");
        private static readonly Regex DriveLetterRegex = new Regex(@"^[a-zA-Z]:[/\\]?$");

        public ManagementObject PartitionObject { get; }
        public ManagementObject? PhysicalDiskObject { get; }
        public ManagementObject? VolumeObject;
        
        public Partition(ManagementObject obj)
        {
            PartitionObject = obj;
            PhysicalDiskObject = PartitionObject.GetRelated("MSFT_Disk").OfType<ManagementObject>().FirstOrDefault();
            VolumeObject = null;
            
            RefreshVolume();
        }

        public int DiskNumber => (int) PartitionObject["DiskNumber"];
        public int PartitionNumber => (int) PartitionObject["PartitionNumber"];
        public char DriveLetter => (char) PartitionObject["DriveLetter"];
        
        public string? Label => (string?) VolumeObject?["FileSystemLabel"];

        public string FriendlyName
        {
            get
            {
                var friendlyPhysical = (string?) PhysicalDiskObject?["FriendlyName"];
                var diskStr = friendlyPhysical ?? $"disk {DiskNumber.ToString()}";
                var partStr = Label != null ? $"\"{Label}\"" : $"Partition {PartitionNumber.ToString()}";
                var letterStr = DriveLetter != 0 ? $"({DriveLetter.ToString()}:) " : "";
                return $"{partStr} {letterStr}on {diskStr}";
            }
        }
        public bool IsOffline => (bool) PartitionObject["IsOffline"];
        public bool IsOnline => !IsOffline;

        public Guid Guid
        {
            get
            {
                var guid = (string?) PartitionObject["Guid"];
                return string.IsNullOrEmpty(guid) ? Guid.Empty : Guid.Parse(guid!);
            }
        }

        public Guid GptType
        {
            get
            {
                var guid = (string?) PartitionObject["GptType"];
                return string.IsNullOrEmpty(guid) ? Guid.Empty : Guid.Parse(guid!);
            }
        }

        public bool IsEfi => EspGuid == GptType;

        public void RefreshVolume()
        {
            VolumeObject = PartitionObject.GetRelated("MSFT_Volume").OfType<ManagementObject>().FirstOrDefault();
        }

        public void Online()
        {
            var result = (string?) PartitionObject.InvokeMethod("Online", Array.Empty<object>());
            if (result != null)
            {
                throw new DiskOperationException(result);
            }
        }
        
        public string[] GetAccessPaths()
        {
            var inParams = PartitionObject.GetMethodParameters("GetAccessPaths");
            var outParams = PartitionObject.InvokeMethod("GetAccessPaths", inParams, null);
            if (outParams == null)
            {
                return Array.Empty<string>();
            }

            var paths = (string[]) outParams["AccessPaths"];
            var mappedPaths = paths.Select(p => p.Replace('?', '.')).ToArray();
            return mappedPaths;
        }

        public void AssignDriveLetter()
        {
            var inParams = PartitionObject.GetMethodParameters("AddAccessPath");
            inParams["AssignDriveLetter"] = true;
            var outParams = PartitionObject.InvokeMethod("AddAccessPath", inParams, null)!;
            var code = Convert.ToInt32(outParams["ReturnValue"]);
            if (code != 0 && code != 42012)
            {
                throw new DiskOperationException("Could not assign drive letter", new Win32Exception(code));
            }
            
            // return code 42012 is ok for some reason, don't ask me why
        }

        public string? GetDriveLetter()
        {
            var paths = GetAccessPaths();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var path in paths)
            {
                var match = DriveLetterRegex.Match(path);
                if (!match.Success) continue;
                
                // found a drive letter
                return path;
            }

            return null;
        }
        
        public override string ToString()
        {
            return FriendlyName;
        }
    }
}