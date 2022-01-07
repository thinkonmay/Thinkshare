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
        [Required]
        public string? CPU { get; set; }

        [Required]
        public string? GPU { get; set; }

        [Required]
        public int? RAMcapacity { get; set; }

        [Required]
        public string? OS { get; set; }

        public string? AgentUrl {get;set;}

        public string? CoreUrl {get;set;}
    }
}