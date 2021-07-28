using SharedHost.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    public class Session : SystemSession
    {
        public int Id { get; set; }

        [NotMapped]
        public virtual new QoE QoE { get; set; }
    }
}