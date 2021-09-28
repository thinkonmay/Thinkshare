using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellExtractor.Model
{
    public class StoragemetricModel
    {
        public int FreeSpave { get; set; }

        public int Size { get; set; }

        public string DeviceID { get; set; } 
    }

    /// <summary>
    /// 
    /// </summary>
    public class StorageDataModel
    {
        public DateTime Time { get; set; }

        public List<StoragemetricModel> StorageList { get; set; }
    }
}
