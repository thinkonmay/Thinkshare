using System;
using System.ComponentModel.DataAnnotations;

namespace WorkerManager.Models
{
    public class Log
    {
        public int WorkerID {get;set;}

        public string Content {get;set;}

        public DateTime LogTime {get;set;}
    }
}