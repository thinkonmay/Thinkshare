using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHost.Models.Device
{
    /// <summary>
    /// Slavedevice is the cloud computer device that allow user to access 
    /// </summary>
    public class WorkerRegisterModel
    {
        /// <summary>
        /// Each slave device defined with an unique ID 
        /// </summary>
        [Required]
        public int PrivateID { get; set; }

        [Required]
        public string? CPU { get; set; }

        [Required]
        public string? GPU { get; set; }

        [Required]
        public int? RAMcapacity { get; set; }

        [Required]
        public string? OS { get; set; }

        public string? LocalIP {get;set;}


    }
}