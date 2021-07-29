namespace SharedHost.Models
{
    public class Message
    {
        public Module From { get; set; }

        public Module To { get; set; }

        public Opcode Opcode { get; set; }

        public string Data { get; set; }
    }

    public class MessageWithID
    {
        public int SlaveID { get; set; }

        public Module From { get; set; }

        public Module To { get; set; }

        public Opcode Opcode { get; set; }

        public string Data { get; set; }
    }

    public enum Opcode
    {
        /// <summary>
        /// 
        /// </summary>
        SESSION_INFORMATION,

        /// <summary>
        /// 
        /// </summary>
        REGISTER_SLAVE,

        /// <summary>
        /// 
        /// </summary>
        SLAVE_ACCEPTED,
        DENY_SLAVE,

        REJECT_SLAVE,

        /// <summary>
        /// 
        /// </summary>
        SESSION_INITIALIZE,
        SESSION_TERMINATION,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,

        /// <summary>
        /// 
        /// </summary>
        CHANGE_MEDIA_MODE,
        CHANGE_RESOLUTION,
        CHANGE_FRAMERATE,
        BITRATE_CALIBRATE,

        /// <summary>
        /// 
        /// </summary>
        COMMAND_LINE_FORWARD,

        /// <summary>
        /// 
        /// </summary>
        EXIT_CODE_REPORT,
        ERROR_REPORT,

        /// <summary>
        /// 
        /// </summary>
        NEW_COMMAND_LINE_SESSION,
        END_COMMAND_LINE_SESSION,
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
