using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedHost.Models.Device;

namespace SharedHost.Models.AWS
{
    public class PortForward
    {
        [Key]
        public int ID { get; set; }

        public int  LocalPort{get;set;}

        public int  InstancePort {get;set;}

        public DateTime Start {get;set;}

        public DateTime? End {get;set;}
    }
}