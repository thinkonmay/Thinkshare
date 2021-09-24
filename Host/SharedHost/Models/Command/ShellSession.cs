using SharedHost.Models.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedHost.Models.Command
{
    public class ShellSession
    {
        public ShellSession(){}

        public ShellSession(ShellOutput output)
        {
            Script = output.Script;

            Output = output.Output;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Script { get; set; }

        public string Output { get; set; }
    }
}
