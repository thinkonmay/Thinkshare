using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedHost.Models.Command
{
    public class ShellSession
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public virtual Slave Device { get; set; }

        public int ProcessID { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public virtual ICollection<CommandLog> Command { get; set; }
    }
}
