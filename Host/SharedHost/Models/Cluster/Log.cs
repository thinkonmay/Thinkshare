using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.Local;


namespace SharedHost.Models.Cluster
{
    public class Log
    {
        [Key]
        public int ID {get;set;}

        public string Content{get;set;}

        public DateTime LogTime {get;set;}
        
        public virtual ClusterWorkerNode worker {get;set;}       
    }
}