using SharedHost.Models.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedHost.Models.Shell
{
    public class ScriptModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Script { get; set; }

        public virtual ICollection<ShellSession> History { get; set; }
    }
}