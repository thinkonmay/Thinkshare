using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedHost.Models
{
    public class Command
    {
        public int Order { get; set; }
<<<<<<< Updated upstream

        public string Commnad { get; set; }
=======
        public string CommandLine { get; set; }
>>>>>>> Stashed changes
    }

    public class CommandResult : Command
    {
        public string Output { get; set; }
    }
}
