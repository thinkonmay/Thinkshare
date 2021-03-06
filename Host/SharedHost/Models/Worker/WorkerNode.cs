using System;
using SharedHost.Models.Shell;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [NotMapped]
        public string? WorkerState {get;set;}
        public DateTime? Register { get; set; }
        public string? CPU { get; set; }
        public string? GPU { get; set; }
        public int? RAMcapacity { get; set; }
        public string? OS { get; set; }
        public string? Name {get;set;}
        public string? User {get;set;}
    }
}