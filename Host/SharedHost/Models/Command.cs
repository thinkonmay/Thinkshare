using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedHost.Models
{
    public class Command
    {
        public int Order { get; set; }
        public string CommandLine { get; set; }
    }

    public class CommandResult : Command
    {
        public string Output { get; set; }
    }
}
