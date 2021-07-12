using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Host.Models
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
            
        SESSION_INITIALIZE_CONFIRM,
        SESSION_TERMINATE_CONFIRM,
        RECONNECT_REMOTE_CONTROL_CONFIRM,
        DISCONNECT_REMOTE_CONTROL_CONFIRM ,

        SESSION_INITIALIZE_FAILED,
        SESSION_TERMINATION_FAILED,
        RECONNECT_REMOTE_CONTROL_FAILED,
        DISCONNECT_REMTOE_CONTROL_FAILED,

        SESSION_INFORMATION_REQUEST ,

        CHANGE_PIPELINE_MODE ,
        BITRATE_CALIBRATE ,

        SLAVE_ERROR ,

        COMMAND_LINE_FOWARD,

        DEVICE_STATE_REQUEST 
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
