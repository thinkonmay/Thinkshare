using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.AWS
{
    public class EC2Instance
    {
        public string Key { set; get; }

        public string IPAdress { set; get; }

        public string InstanceID { get; set; }

        public string InstanceName {  set; get; } 

        public string PrivateIP { get;set; }
    }
}
