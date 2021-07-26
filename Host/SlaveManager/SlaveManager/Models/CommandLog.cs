using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class CommandLog : CommandResult
    {
        public int Id { get; set; }

        public virtual Slave Machine { get; set; }
    }
}
