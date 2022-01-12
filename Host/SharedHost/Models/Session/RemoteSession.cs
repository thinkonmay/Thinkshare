using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.Device;
using SharedHost.Models.User;

namespace SharedHost.Models.Session
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteSession
    { 
        [Key]
        public int ID { get; set; }

        [Required]
        public int ClientId {get;set;}

        [ForeignKey("ClientId")]
        public virtual UserAccount Client { get; set; }
        
        [Required]
        public int WorkerID {get;set;}

        [ForeignKey("WorkerID")]
        public virtual WorkerNode Worker { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}