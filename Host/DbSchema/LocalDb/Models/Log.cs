using System;
using System.ComponentModel.DataAnnotations;

namespace DbSchema.LocalDb.Models
{
    public class Log
    {
        [Key]
        public int ID {get;set;}

        public string Content{get;set;}

        public DateTime LogTime {get;set;}
        
        public virtual ClusterWorkerNode? worker {get;set;}       
    }
}