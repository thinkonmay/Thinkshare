using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalling.Models
{
    public class WebSocketMessage
    {
        public string RequestType { get; set; }
        public int SubjectId { get; set; }
        public string Content { get; set; }
        public string Result { get; set; }

        public const string SLAVE_REQUEST = "SLAVEREQUEST";
        public const string CLIENT_REQUEST = "CLIENTREQUEST";
    }
}
