﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.AWS
{
    public class ClusterInstance : EC2Instance
    {
        [Key]
        public int ID {get;set;}

        public string TurnUser {get;set;}

        public string TurnPassword {get;set;}

        public DateTime? Registered {get;set;}

        public ClusterInstance(EC2Instance instance)
        {
            IPAdress = instance.IPAdress;
            InstanceID = instance.InstanceID;
            InstanceName = instance.InstanceName;
            PrivateIP = instance.PrivateIP;
        }
        public ClusterInstance() { }
    }
}
