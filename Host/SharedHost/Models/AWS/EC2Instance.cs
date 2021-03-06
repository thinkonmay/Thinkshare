using System;

namespace SharedHost.Models.AWS
{
    public class EC2Instance
    {
        public string IPAdress { set; get; }

        public string InstanceID { get; set; }

        public string InstanceName {  set; get; } 

        public string PrivateIP { get;set; }

        public virtual EC2KeyPair keyPair { get; set; }

        public DateTime Start {get;set;}

        public DateTime? End {get;set;}
    }
}
