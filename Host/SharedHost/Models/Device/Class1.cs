using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Device
{
    public class HostConfiguration
    {
        public int SlaveID { get; set; }
        public string HostUrl {get;set;}
        public bool DisableSSL {get;set;}
    }
}
