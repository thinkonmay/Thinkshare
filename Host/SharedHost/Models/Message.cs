using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SharedHost.Models
{
    public class Message
    {
        public Module From { get; set; }

        public Module To { get; set; }

        public Opcode Opcode { get; set; }

        public string Data { get; set; }
    }

    public enum Opcode
    {
        SESSION_INFORMATION = 1,

        REGISTER_SLAVE,

        SLAVE_ACCEPTED,
        DENY_SLAVE,

        REJECT_SLAVE ,

        UPDATE_SLAVE_STATE ,

        SESSION_INITIALIZE,
        SESSION_TERMINATION,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMTOE_CONTROL ,

        SESSION_INFORMATION_REQUEST ,

    	CHANGE_MEDIA_MODE,						
    	CHANGE_RESOLUTION,						
    	CHANGE_FRAMERATE,						
    	BITRATE_CALIBRATE,						

        COMMAND_LINE_FOWARD,

        EXIT_CODE_REPORT,						
        ERROR_REPORT							
    }

    public enum Module
    {
        CORE_MODULE,
        CLIENT_MODULE,
        LOADER_MODULE,
        AGENT_MODULE,
        HOST_MODULE
    }
}
