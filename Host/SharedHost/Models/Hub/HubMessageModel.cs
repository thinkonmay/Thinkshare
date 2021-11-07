using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.Hub
{
    public class HubMessage
    {
        public string EventName {  get; set; }

        public string Message { get; set; }
    }
}
