using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models
{
    public class SessionViewModel
    {
        public ClientSession clientSession { get; set; }

        public string HostUrl {get;set;}

        public int ClientID {get;set;}
    }
}
