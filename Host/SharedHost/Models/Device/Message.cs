namespace SharedHost.Models.Device
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

        public int From { get; set; }

        public int To { get; set; }

        public int Opcode { get; set; }

        public string Data { get; set; }
    }

    public enum Opcode
    {
        SESSION_INFORMATION	,
        REGISTER_SLAVE	,
        SLAVE_ACCEPTED	,
        DENY_SLAVE	,
        REJECT_SLAVE,

        SESSION_INITIALIZE,
        SESSION_TERMINATE,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,
        QOE_REPORT,
        RESET_QOE,
        COMMAND_LINE_FORWARD,
        SESSION_CORE_EXIT,
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
        HOST_MODULE,
    }

    public enum HidOpcode
    {
        KEYUP,
        KEYDOWN,
        MOUSE_WHEEL,
        MOUSE_MOVE,
        MOUSE_UP,
        MOUSE_DOWN,
    }
}
