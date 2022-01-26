using System;

namespace SharedHost.Logging
{
    public class GenericLogModel
    {
        public string Log {get;set;}

        public string Source {get;set;}

        public string Type {get;set;}

        public DateTime timestamp {get;set;}
    }

    public class ErrorLogModel
    {
        public DateTime timestamp {get;set;}

        public string Source {get;set;}

        public string Message {get;set;}

        public string StackTrace {get;set;}

        public string Log {get;set;}
    }

    public class ClusterLogModel
    {
        public DateTime timestamp {get;set;}

        public int ClusterID {get;set;}

        public string Log{get;set;}
    }
}