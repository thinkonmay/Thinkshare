using RestSharp;
using System;
using System.Threading.Tasks;
using System.Net;

namespace WorkerManager.Interfaces
{
    public interface ILog
    {
        void Information(string information);
        void Error(string message, Exception exception);
        void Warning(string message);
        void Worker(string information, string WorkerID);
    }
}