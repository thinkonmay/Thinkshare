using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Conductor.Models
{
    public class Storage
    {
        public int StorageID { get; set; }

        public string DriveName { get; set; }

        public string Url { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}