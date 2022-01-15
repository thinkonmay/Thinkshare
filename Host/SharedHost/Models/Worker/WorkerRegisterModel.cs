using System.ComponentModel.DataAnnotations;
using SharedHost.Models.AWS;

namespace SharedHost.Models.Device
{
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

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? User { get; set; }

        public string? AgentUrl {get;set;}
    }
}