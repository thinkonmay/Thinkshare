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
    public class RemoteSession : SessionBase
    { 
        public RemoteSession() { }

        public RemoteSession(SystemConfig config)
        {
            SignallingUrl = config.SignallingWs;
        }

        /// <summary>
        /// preserved for database insert,
        ///  should only be used by admin service to
        /// insert database
        /// </summary>
        /// <value></value>
        [Required]
        public int ClientId {get;set;}

        [ForeignKey("ClientId")]
        public virtual UserAccount Client { get; set; }
        
        /// <summary>
        /// preserved for database insert,
        ///  should only be used by admin service to
        /// insert database
        /// </summary>
        /// <value></value>
        [Required]
        public int SlaveID {get;set;}

        [ForeignKey("SlaveID")]
        public virtual WorkerNode Worker { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}