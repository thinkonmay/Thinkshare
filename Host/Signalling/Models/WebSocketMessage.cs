using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Signalling.Models
{
    public class WebSocketMessage
    {
        public string RequestType { get; set; }
        public string Content { get; set; }
        public string Result { get; set; }
    }

    public class WebSocketMessageResult
    {
        public const string OFFER_SDP =  "OFFER_SDP";
        public const string OFFER_ICE =  "OFFER_ICE";

        public const string RESULT_ACCEPTED = "SESSION_ACCEPTED";
        public const string RESULT_REJECTED = "SESSION_REJECTED";
    }
}
