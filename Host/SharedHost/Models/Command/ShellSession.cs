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
            Script = output.Script.Remove(0,1).Replace("\n", "").Replace("\r", "").Replace(@"[^\u0020-\u007F]","");

            Output = output.Output.Remove(0,1).Replace("\n", "").Replace("\r", "").Replace(@"[^\u0020-\u007F]","");
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Script { get; set; }

        public string Output { get; set; }
    }
}
