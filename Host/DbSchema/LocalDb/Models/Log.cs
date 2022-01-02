using System;
using System.ComponentModel.DataAnnotations;

namespace DbSchema.LocalDb.Models
{
    public class Log
    {
        public int ID {get;set;}

        public int WorkerID {get;set;}

        public string Content {get;set;}

        public DateTime LogTime {get;set;}
    }
}