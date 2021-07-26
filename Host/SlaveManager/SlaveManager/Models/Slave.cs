using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class Slave
    {
        public int Id { get; set; }
        public string CPU { get; set; }
        public string GPU { get; set; }
        public int RAMcapacity { get; set; }
        public string OS { get; set; }

        public virtual ICollection<Telemetry> Telemetry { get; set; }
        public virtual ICollection<CommandLog> CommandLogs { get; set; }
    }

    public class Telemetry
    {
        public int CPUusage;
        public int GPUusage;
        public int RAMusage;

        public virtual Slave Device { get; set; }
    }
}