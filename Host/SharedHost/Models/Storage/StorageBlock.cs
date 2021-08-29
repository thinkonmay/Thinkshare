using SharedHost.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Conductor.Models
{
    /// <summary>
    /// WHen user access cloud storage service, the storage will be divided into different Block,
    /// each block has an different identification
    /// </summary>
    public class StorageBlock
    {
        [Key]
        public int StorageID { get; set; }

        public string DriveName { get; set; }

        public int Capacity { get; set; }

        /// <summary>
        /// Key to access storage block in cluster,
        /// different between storage technology
        /// </summary>
        /// TODO: To be added later. Comment out to avoid database problems
        //public StorageBlockIdentification identification { get; set; }
    }
}