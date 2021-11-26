using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHost.Models.Device
{
    /// <summary>
    /// Slavedevice is the cloud computer device that allow user to access 
    /// </summary>
    public class WorkerNode
    {
        /// <summary>
        /// Each slave device defined with an unique ID 
        /// </summary>
        [Key]
        public int ID { get; set; }

        public DateTime? Register { get; set; }

        public string? CPU { get; set; }

        public string? GPU { get; set; }

        public int? RAMcapacity { get; set; }

        public string? OS { get; set; }
    }
}