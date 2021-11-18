namespace SharedHost.Models.Device
{
    public class Message
    {
        public Module From { get; set; }

        public Module To { get; set; }

        public Opcode Opcode { get; set; }

        public string? Data { get; set; }

        public int? WorkerID { get; set; }
    }

    public enum Opcode
    {
        SESSION_INITIALIZE,
        SESSION_TERMINATE,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,

        QOE_REPORT,
        RESET_QOE,
        SESSION_CORE_EXIT,
        ERROR_REPORT,

        SHELL_SESSION,

        STATE_SYNCING,

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
        CLUSTER_MODULE,
    }
}
