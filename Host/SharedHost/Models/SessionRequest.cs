using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models
{
    public class SessionRequest
    {
        public int ClientId { get; set; }
        public int SlaveId { get; set; }
    }
}
