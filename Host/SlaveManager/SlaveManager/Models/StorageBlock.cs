using SharedHost.Models;
using System.Collections.Generic;

namespace SlaveManager.Models
{
    /// <summary>
    /// WHen user access cloud storage service, the storage will be divided into different Block,
    /// each block has an different identification
    /// </summary>
    public class StorageBlock
    {
        public int StorageID { get; set; }

        public string DriveName { get; set; }

        public int Capacity { get; set; }

        /// <summary>
        /// Key to access storage block in cluster,
        /// different between storage technology
        /// </summary>
        public StorageBlockIdentification identification { get; set; }

        public virtual ICollection<Client> servingClient { get; set; }

        public virtual ICollection<Slave>? servingSlave { get; set; }

        public virtual ICollection<Cluster> storingCluster { get; set; }
    }
}