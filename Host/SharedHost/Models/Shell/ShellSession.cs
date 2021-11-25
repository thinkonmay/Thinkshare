using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedHost.Models.Shell
{
    public class ShellSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Script { get; set; }

        public string Output { get; set; }

        /// <summary>
        /// preserved for database insert,
        ///  should only be used by admin service to
        /// insert database
        /// </summary>
        /// <value></value>
        [Required]
        public int WorkerID { get; set; }

        [ForeignKey("WorkerID")]
        public virtual WorkerNode Worker { get; set; }

        [Required]
        public int ModelID { get; set; }
        
        [ForeignKey("ModelID")]
        public virtual ScriptModel Model { get; set; }
    }
}
