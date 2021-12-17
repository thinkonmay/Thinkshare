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

        public string Output { get; set; }

        public virtual ScriptModel Model { get; set; }
    }
}
