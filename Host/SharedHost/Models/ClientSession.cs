using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace SharedHost.Models
{
    public class ClientSession
    {
        public int SessionClientID { get; set; }

        public string SignallingUrl { get; set; }

        public string StunServer { get; set; }

        public bool ClientOffer { get; set; }

        public QoE QoE { get; set; }
    }
}