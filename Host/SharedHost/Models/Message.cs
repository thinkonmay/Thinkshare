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
        SESSION_INFORMATION,

        REGISTER_SLAVE,

        SLAVE_ACCEPTED,
        DENY_SLAVE,

        REJECT_SLAVE,

        SESSION_INITIALIZE,
        SESSION_TERMINATE,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,

        QOE_REPORT,
        RESET_QOE,

        COMMAND_LINE_FORWARD,

        EXIT_CODE_REPORT,
        ERROR_REPORT,

        NEW_COMMAND_LINE_SESSION,
        END_COMMAND_LINE_SESSION,

        FILE_TRANSFER_SERVICE,
        CLIPBOARD_SERVICE
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
