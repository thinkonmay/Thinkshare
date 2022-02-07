using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHost.Models.Shell
{
    public class ShellSession
    {
        public DateTime Time { get; set; }

        public string Output { get; set; }

        public int ModelID { get; set; }
    }
}
